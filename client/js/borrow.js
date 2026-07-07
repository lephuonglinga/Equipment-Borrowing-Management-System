let allRequests = [];
let activeTab = "pending";

const TAB_STATUS = {
  pending: ["Pending"],
  pickup: ["Approved"],
  active: ["InProgress", "Overdue"],
  history: ["Completed", "Rejected", "Cancelled"]
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

const HANDOVER_CONDITIONS = ["Good", "Fair", "Damaged"];
const RETURN_CONDITIONS = ["Good", "Fair", "Damaged", "Lost"];

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

function patchBorrowRequest(id, body) {
  return apiRequest({ url: "/api/borrow-requests/" + id, method: "PATCH", body: body });
}

function filterByTab(requests) {
  return requests.filter(function (r) {
    return requestMatchesTab(r, activeTab);
  });
}

function updateTabLabels() {
  $("#tabPending").text("Chờ duyệt (" + countByTab("pending") + ")");
  $("#tabPickup").text("Chờ bàn giao (" + countByTab("pickup") + ")");
  $("#tabActive").text("Đang mượn (" + countByTab("active") + ")");
  $("#tabHistory").text("Lịch sử (" + countByTab("history") + ")");
}

function updatePageHeader() {
  const staff = isStaffOrAdmin();
  if (staff) {
    $("#pageTitle").text("Duyệt mượn");
    $("#pageSubtitle").text(
      "Duyệt đơn → Bàn giao (ghi tình trạng) → Nhận trả. Thiết bị Reserved ngay khi gửi yêu cầu, Borrowed sau bàn giao."
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
  let html = '<ul class="item-list item-list-detailed">';
  items.forEach(function (item) {
    html += "<li>";
    html += "<strong>" + escapeHtml(item.equipmentName) + "</strong> · " + escapeHtml(item.serialNumber);
    if (item.conditionAtBorrow) {
      html += '<div class="item-condition">Giao: ' + renderConditionBadge(item.conditionAtBorrow);
      if (item.handoverNote) {
        html += " — " + escapeHtml(item.handoverNote);
      }
      html += "</div>";
    }
    if (item.conditionAtReturn) {
      html += '<div class="item-condition">Trả: ' + renderConditionBadge(item.conditionAtReturn);
      if (item.conditionAtBorrow && conditionWorsened(item.conditionAtBorrow, item.conditionAtReturn)) {
        html += ' <span class="condition-worse">(xấu hơn lúc giao)</span>';
      }
      if (item.returnNote) {
        html += " — " + escapeHtml(item.returnNote);
      }
      html += "</div>";
    }
    html += "</li>";
  });
  html += "</ul>";
  return html;
}

function conditionWorsened(atBorrow, atReturn) {
  const order = { Good: 1, Fair: 2, Damaged: 3, Lost: 4, Compensated: 5 };
  return (order[atReturn] || 0) > (order[atBorrow] || 0);
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

  if (!staff && (status === "Pending" || status === "Approved")) {
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

  if (staff && status === "Approved") {
    html +=
      '<div class="request-card-actions"><button type="button" class="btn btn-primary btn-sm btn-handover" data-id="' +
      r.id +
      '">Bàn giao</button></div>';
  }

  if (staff && (status === "InProgress" || status === "Overdue")) {
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
    const labels = {
      pending: "chờ duyệt",
      pickup: "chờ bàn giao",
      active: "đang mượn",
      history: "lịch sử"
    };
    let msg = "Không có yêu cầu " + labels[activeTab] + ".";
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
    patchBorrowRequest(id, { status: "Cancelled" })
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
    patchBorrowRequest(id, { status: "Approved" })
      .done(function () {
        showAlert($("#listAlert"), "Đã duyệt — chờ bàn giao thiết bị.", "success");
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
    patchBorrowRequest(id, { status: "Rejected", rejectReason: reason.trim() })
      .done(function () {
        showAlert($("#listAlert"), "Đã từ chối yêu cầu.", "success");
        fetchBorrowList();
      })
      .fail(function (xhr) {
        showAlert($("#listAlert"), getErrorMessage(xhr), "error");
      });
  });

  $(".btn-handover").on("click", function () {
    openDetail(parseInt($(this).data("id"), 10), "handover");
  });

  $(".btn-return").on("click", function () {
    openDetail(parseInt($(this).data("id"), 10), "return");
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

function conditionSelectOptions(conditions, selected) {
  let html = "";
  conditions.forEach(function (c) {
    html += '<option value="' + c + '"' + (selected === c ? " selected" : "") + ">" + c + "</option>";
  });
  return html;
}

function openHandoverForm(r) {
  let formHtml = '<div class="section-title" style="margin-top:0;">Kiểm tra tình trạng khi bàn giao</div>';
  (r.items || []).forEach(function (item) {
    formHtml += '<div class="form-group handover-item" data-eq="' + item.equipmentId + '">';
    formHtml += "<label>" + escapeHtml(item.equipmentName) + " · " + escapeHtml(item.serialNumber) + "</label>";
    formHtml +=
      '<select class="handover-condition">' +
      conditionSelectOptions(HANDOVER_CONDITIONS, "Good") +
      "</select>";
    formHtml += '<input type="text" class="handover-note" placeholder="Ghi chú (tùy chọn)">';
    formHtml += "</div>";
  });
  formHtml +=
    '<div style="margin-top:12px;text-align:right;"><button type="button" class="btn btn-primary" id="btnConfirmHandover">Xác nhận bàn giao</button></div>';
  $("#detailBody").html(formHtml);
  $("#detailActions").html('<button type="button" class="btn btn-ghost" data-close>Đóng</button>');

  $("#btnConfirmHandover").on("click", function () {
    const items = [];
    $(".handover-item").each(function () {
      items.push({
        equipmentId: parseInt($(this).data("eq"), 10),
        conditionAtBorrow: $(this).find(".handover-condition").val(),
        note: $(this).find(".handover-note").val().trim() || null
      });
    });
    patchBorrowRequest(r.id, {
      status: "InProgress",
      items: items
    })
      .done(function () {
        $("#detailModal").removeClass("open");
        showAlert($("#listAlert"), "Đã bàn giao thiết bị.", "success");
        fetchBorrowList();
      })
      .fail(function (xhr) {
        alert(getErrorMessage(xhr));
      });
  });
}

function openReturnForm(r) {
  let formHtml = '<div class="section-title" style="margin-top:0;">Kiểm tra tình trạng khi trả</div>';
  formHtml += '<div class="form-group"><label>Ghi chú staff</label><input type="text" id="staffNote"></div>';
  (r.items || []).forEach(function (item) {
    const defaultCond = item.conditionAtBorrow || "Good";
    formHtml += '<div class="form-group return-item" data-eq="' + item.equipmentId + '">';
    formHtml += "<label>" + escapeHtml(item.equipmentName);
    if (item.conditionAtBorrow) {
      formHtml += " (giao: " + escapeHtml(item.conditionAtBorrow) + ")";
    }
    formHtml += "</label>";
    formHtml +=
      '<select class="return-condition">' +
      conditionSelectOptions(RETURN_CONDITIONS, defaultCond) +
      "</select>";
    formHtml += '<input type="text" class="return-note" placeholder="Ghi chú (tùy chọn)">';
    formHtml += "</div>";
  });
  formHtml +=
    '<div style="margin-top:12px;text-align:right;"><button type="button" class="btn btn-primary" id="btnConfirmReturn">Xác nhận trả</button></div>';
  $("#detailBody").html(formHtml);
  $("#detailActions").html('<button type="button" class="btn btn-ghost" data-close>Đóng</button>');

  $("#btnConfirmReturn").on("click", function () {
    const items = [];
    $(".return-item").each(function () {
      items.push({
        equipmentId: parseInt($(this).data("eq"), 10),
        conditionAtReturn: $(this).find(".return-condition").val(),
        note: $(this).find(".return-note").val().trim() || null
      });
    });
    patchBorrowRequest(r.id, {
      status: "Completed",
      staffNote: $("#staffNote").val().trim() || null,
      items: items
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

function openDetail(id, mode) {
  apiRequest({ url: "/api/borrow-requests/" + id }).done(function (r) {
    $("#detailId").text(r.id);
    if (mode === "handover" && isStaffOrAdmin()) {
      openHandoverForm(r);
    } else if (mode === "return" && isStaffOrAdmin()) {
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
