let allRequests = [];
let activeTab = "pending";

const TAB_STATUS = {
  pending: ["Pending"],
  active: ["Approved", "InProgress", "Overdue"],
  history: ["Completed", "Rejected", "Cancelled", "Returned"]
};

const STATUS_BY_NUMBER = {
  1: "Pending",
  2: "Approved",
  3: "Rejected",
  4: "Cancelled",
  5: "InProgress",
  6: "Returned",
  7: "Completed",
  8: "Overdue"
};

function normalizeBorrowStatus(status) {
  if (status == null) return "";
  if (typeof status === "number") {
    return STATUS_BY_NUMBER[status] || String(status);
  }
  return String(status).trim();
}

function requestMatchesTab(request, tab) {
  const status = normalizeBorrowStatus(request.status);
  const allowed = TAB_STATUS[tab] || [];
  return allowed.indexOf(status) >= 0;
}

function countByTab(tab) {
  return allRequests.filter(function (r) {
    return requestMatchesTab(r, tab);
  }).length;
}

function loadBorrowRequests() {
  return apiRequest({ url: "/api/borrow-requests" });
}

function filterByTab(requests) {
  return requests.filter(function (r) {
    return requestMatchesTab(r, activeTab);
  });
}

function updateTabLabels() {
  $("#tabPending").text("Chờ duyệt (" + countByTab("pending") + ")");
  $("#tabActive").text("Đang mượn (" + countByTab("active") + ")");
  $("#tabHistory").text("Lịch sử (" + countByTab("history") + ")");
}

function updatePageHeader() {
  const staff = isStaffOrAdmin();
  if (staff) {
    $("#pageTitle").text("Duyệt mượn");
    $("#pageSubtitle").text(
      "Yêu cầu chờ duyệt (tab Chờ duyệt) — khác với trạng thái thiết bị trên Equipments."
    );
    $("#btnBorrowMore").hide();
  } else {
    $("#pageTitle").text("Yêu cầu mượn");
    $("#pageSubtitle").text("Theo dõi trạng thái các yêu cầu mượn thiết bị.");
    $("#btnBorrowMore").show();
  }
}

function renderItemsList(items) {
  if (!items || items.length === 0) return "<p>Không có thiết bị trong yêu cầu.</p>";
  let html = '<ul class="item-list">';
  items.forEach(function (item) {
    html +=
      "<li><strong>" +
      escapeHtml(item.equipmentName) +
      "</strong> · " +
      escapeHtml(item.serialNumber) +
      "</li>";
  });
  html += "</ul>";
  return html;
}

function renderRequestCard(r) {
  const staff = isStaffOrAdmin();
  const status = normalizeBorrowStatus(r.status);
  let html = '<div class="request-card">';
  html += '<div class="request-card-head">';
  html += "<div><strong>#" + r.id + "</strong> " + renderStatusBadge(status, "borrow") + "</div>";
  html += '<button type="button" class="btn btn-ghost btn-sm btn-detail" data-id="' + r.id + '">Chi tiết</button>';
  html += "</div>";

  if (staff) {
    html += '<div class="request-meta"><i class="fa-solid fa-user"></i> ' + escapeHtml(r.userName) + "</div>";
  }

  html +=
    '<div class="request-meta">Mượn: ' +
    formatDate(r.borrowDate) +
    " · Trả: " +
    formatDate(r.expectedReturnDate) +
    "</div>";
  html += '<div class="request-meta">' + escapeHtml(r.purpose) + "</div>";
  html += renderItemsList(r.items);

  if (!staff && status === "Pending") {
    html +=
      '<div class="request-card-actions"><button type="button" class="btn btn-danger btn-sm btn-cancel" data-id="' +
      r.id +
      '">Hủy yêu cầu</button></div>';
  }

  if (staff && status === "Pending") {
    html += '<div class="request-card-actions">';
    html +=
      '<button type="button" class="btn btn-primary btn-sm btn-approve" data-id="' + r.id + '">Duyệt</button> ';
    html +=
      '<button type="button" class="btn btn-danger btn-sm btn-reject" data-id="' + r.id + '">Từ chối</button>';
    html += "</div>";
  }

  if (staff && (status === "Approved" || status === "InProgress" || status === "Overdue")) {
    html +=
      '<div class="request-card-actions"><button type="button" class="btn btn-primary btn-sm btn-return" data-id="' +
      r.id +
      '">Xác nhận trả</button></div>';
  }

  html += "</div>";
  return html;
}

function renderBorrowList() {
  updateTabLabels();
  const filtered = filterByTab(allRequests);

  if (filtered.length === 0) {
    const labels = { pending: "chờ duyệt", active: "đang mượn", history: "lịch sử" };
    let msg = "Không có yêu cầu " + labels[activeTab] + ".";
    if (isStaffOrAdmin() && activeTab === "pending" && allRequests.length > 0) {
      msg += " Thử tab Đang mượn hoặc Lịch sử — thiết bị trên Equipments vẫn Available cho đến khi yêu cầu được duyệt.";
    }
    $("#borrowList").html('<p class="empty-state">' + msg + "</p>");
    return;
  }

  $("#borrowList").html(filtered.map(renderRequestCard).join(""));
  bindRequestActions();
}

function bindRequestActions() {
  $(".btn-detail").on("click", function () {
    openDetail(parseInt($(this).data("id"), 10));
  });

  $(".btn-cancel").on("click", function () {
    const id = parseInt($(this).data("id"), 10);
    if (!confirm("Hủy yêu cầu #" + id + "?")) return;
    apiRequest({ url: "/api/borrow-requests/" + id + "/cancel", method: "PUT" })
      .done(function () {
        showAlert($("#listAlert"), "Đã hủy yêu cầu.", "success");
        fetchBorrowList();
      })
      .fail(function (xhr) {
        showAlert($("#listAlert"), getErrorMessage(xhr), "error");
      });
  });

  $(".btn-approve").on("click", function () {
    const id = parseInt($(this).data("id"), 10);
    apiRequest({ url: "/api/borrow-requests/" + id + "/approve", method: "PUT" })
      .done(function () {
        showAlert($("#listAlert"), "Đã duyệt yêu cầu.", "success");
        fetchBorrowList();
      })
      .fail(function (xhr) {
        showAlert($("#listAlert"), getErrorMessage(xhr), "error");
      });
  });

  $(".btn-reject").on("click", function () {
    const id = parseInt($(this).data("id"), 10);
    const reason = prompt("Lý do từ chối:");
    if (!reason || !reason.trim()) return;
    apiRequest({
      url: "/api/borrow-requests/" + id + "/reject",
      method: "PUT",
      body: { rejectReason: reason.trim() }
    })
      .done(function () {
        showAlert($("#listAlert"), "Đã từ chối yêu cầu.", "success");
        fetchBorrowList();
      })
      .fail(function (xhr) {
        showAlert($("#listAlert"), getErrorMessage(xhr), "error");
      });
  });

  $(".btn-return").on("click", function () {
    openDetail(parseInt($(this).data("id"), 10), true);
  });
}

function fetchBorrowList() {
  $("#borrowList").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải...</p>');
  loadBorrowRequests()
    .done(function (data) {
      allRequests = Array.isArray(data) ? data : [];
      renderBorrowList();
    })
    .fail(function (xhr) {
      $("#borrowList").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

function renderDetailBody(r) {
  const status = normalizeBorrowStatus(r.status);
  let html = '<div class="detail-grid">';
  html += '<div><div class="label">Người mượn</div>' + escapeHtml(r.userName) + "</div>";
  html += '<div><div class="label">Trạng thái</div>' + renderStatusBadge(status, "borrow") + "</div>";
  html += '<div><div class="label">Ngày mượn</div>' + formatDate(r.borrowDate) + "</div>";
  html += '<div><div class="label">Trả dự kiến</div>' + formatDate(r.expectedReturnDate) + "</div>";
  html += '<div class="full"><div class="label">Mục đích</div>' + escapeHtml(r.purpose) + "</div>";
  if (r.rejectReason) {
    html += '<div class="full"><div class="label">Lý do từ chối</div>' + escapeHtml(r.rejectReason) + "</div>";
  }
  html += "</div>";
  html += '<div class="section-title" style="margin-top:0;">Thiết bị</div>';
  html += renderItemsList(r.items);
  return html;
}

function openReturnForm(r) {
  let formHtml = '<div class="section-title" style="margin-top:0;">Tình trạng khi trả</div>';
  formHtml += '<div class="form-group"><label>Ghi chú staff</label><input type="text" id="staffNote"></div>';
  (r.items || []).forEach(function (item) {
    formHtml += '<div class="form-group"><label>' + escapeHtml(item.equipmentName) + "</label>";
    formHtml +=
      '<select class="return-condition" data-eq="' +
      item.equipmentId +
      '"><option value="Good">Good</option><option value="Fair">Fair</option><option value="Damaged">Damaged</option><option value="Lost">Lost</option></select></div>';
  });
  formHtml +=
    '<div style="margin-top:12px;text-align:right;"><button type="button" class="btn btn-primary" id="btnConfirmReturn">Xác nhận trả</button></div>';
  $("#detailBody").html(formHtml);
  $("#detailActions").html('<button type="button" class="btn btn-ghost" data-close>Đóng</button>');

  $("#btnConfirmReturn").on("click", function () {
    const items = [];
    $(".return-condition").each(function () {
      items.push({
        equipmentId: parseInt($(this).data("eq"), 10),
        conditionAtReturn: $(this).val()
      });
    });
    apiRequest({
      url: "/api/borrow-requests/" + r.id + "/return",
      method: "PUT",
      body: { staffNote: $("#staffNote").val().trim() || null, items: items }
    })
      .done(function () {
        $("#detailModal").removeClass("open");
        showAlert($("#listAlert"), "Đã xác nhận trả thiết bị.", "success");
        fetchBorrowList();
      })
      .fail(function (xhr) {
        alert(getErrorMessage(xhr));
      });
  });
}

function openDetail(id, forReturn) {
  apiRequest({ url: "/api/borrow-requests/" + id }).done(function (r) {
    $("#detailId").text(r.id);
    if (forReturn && isStaffOrAdmin()) {
      openReturnForm(r);
    } else {
      $("#detailBody").html(renderDetailBody(r));
      $("#detailActions").html('<button type="button" class="btn btn-ghost" data-close>Đóng</button>');
    }
    $("#detailModal").addClass("open");
  });
}

function switchTab(tab) {
  activeTab = tab;
  $("#borrowTabs .tab-btn").removeClass("active");
  $('#borrowTabs .tab-btn[data-tab="' + tab + '"]').addClass("active");
  renderBorrowList();
}

$(document).ready(function () {
  if (!requireAuth()) return;
  renderAuthNav("borrow");
  updatePageHeader();

  const params = new URLSearchParams(window.location.search);
  const tabParam = params.get("tab");
  if (tabParam && TAB_STATUS[tabParam]) {
    activeTab = tabParam;
    $("#borrowTabs .tab-btn").removeClass("active");
    $('#borrowTabs .tab-btn[data-tab="' + tabParam + '"]').addClass("active");
  } else if (isStaffOrAdmin() && params.get("tab") === null) {
    activeTab = "pending";
  }

  fetchBorrowList();

  $("#borrowTabs .tab-btn").on("click", function () {
    switchTab($(this).data("tab"));
  });

  $("[data-close]").on("click", function () {
    $(this).closest(".modal-overlay").removeClass("open");
  });
});
