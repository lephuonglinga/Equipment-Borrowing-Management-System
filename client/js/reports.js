function renderDashboard(data) {
  const eq = data.equipmentByStatus || {};
  let html = '<div class="stats-grid">';
  html += statCard("Tổng thiết bị", eq.total);
  html += statCard("Available", eq.available);
  html += statCard("Borrowed", eq.borrowed);
  html += statCard("Maintenance", eq.maintenance);
  html += statCard("Quá hạn", data.overdueRequestCount, true);
  html +=
    '<a href="manage.html?currentCondition=Damaged" class="stat-card-link">' +
    statCard("Đang hư hỏng", data.damagedEquipmentCount, true) +
    "</a>";
  html +=
    '<a href="manage.html?status=Lost" class="stat-card-link">' +
    statCard("Đang mất", data.lostEquipmentCount, true) +
    "</a>";
  html += "</div>";

  if (data.borrowRequestsByStatus && data.borrowRequestsByStatus.length) {
    html += '<div class="section-title" style="margin-top:0;">Yêu cầu mượn theo trạng thái</div>';
    html += '<div class="data-table-wrap"><table class="data-table"><thead><tr><th>Status</th><th>Số lượng</th></tr></thead><tbody>';
    data.borrowRequestsByStatus.forEach(function (row) {
      html += "<tr><td>" + renderStatusBadge(row.status, "borrow") + "</td><td>" + row.count + "</td></tr>";
    });
    html += "</tbody></table></div>";
  }

  if (data.mostBorrowedEquipment && data.mostBorrowedEquipment.length) {
    html += '<div class="section-title">Thiết bị mượn nhiều nhất</div>';
    html += '<div class="data-table-wrap"><table class="data-table"><thead><tr><th>Thiết bị</th><th>Serial</th><th>Lượt mượn</th></tr></thead><tbody>';
    data.mostBorrowedEquipment.forEach(function (row) {
      html +=
        "<tr><td>" +
        escapeHtml(row.equipmentName) +
        "</td><td>" +
        escapeHtml(row.serialNumber) +
        "</td><td>" +
        row.borrowCount +
        "</td></tr>";
    });
    html += "</tbody></table></div>";
  }

  $("#dashboardStats").html(html);
}

function statCard(label, value, accent) {
  return (
    '<div class="stat-card' +
    (accent ? " accent" : "") +
    '"><div class="label">' +
    escapeHtml(label) +
    '</div><div class="value">' +
    (value != null ? value : "—") +
    "</div></div>"
  );
}

function renderSummary(data) {
  let html = '<div class="stats-grid">';
  html += statCard("Tổng yêu cầu", data.totalRequests);
  html += statCard("Hoàn thành", data.completedRequests);
  html += statCard("Đang active", data.activeRequests);
  html += statCard("Từ chối", data.rejectedRequests);
  html += statCard("Đã hủy", data.cancelledRequests);
  html += "</div>";

  if (data.requestsByStatus && data.requestsByStatus.length) {
    html += '<div class="data-table-wrap"><table class="data-table"><thead><tr><th>Status</th><th>Số lượng</th></tr></thead><tbody>';
    data.requestsByStatus.forEach(function (row) {
      html += "<tr><td>" + renderStatusBadge(row.status, "borrow") + "</td><td>" + row.count + "</td></tr>";
    });
    html += "</tbody></table></div>";
  } else {
    html += '<p class="empty-state">Không có dữ liệu trong khoảng ngày đã chọn.</p>';
  }

  $("#borrowSummary").html(html);
}

function renderOverdue(list) {
  if (!list || list.length === 0) {
    $("#overdueTable").html('<p class="empty-state">Không có yêu cầu quá hạn.</p>');
    return;
  }

  let html = '<table class="data-table"><thead><tr>';
  html += "<th>ID</th><th>Người mượn</th><th>Email</th><th>Trả dự kiến</th><th>Quá (ngày)</th><th>Thiết bị</th>";
  html += "</tr></thead><tbody>";

  list.forEach(function (r) {
    const items = (r.items || [])
      .map(function (i) {
        return escapeHtml(i.equipmentName);
      })
      .join(", ");
    html += "<tr>";
    html += "<td>#" + r.id + "</td>";
    html += "<td>" + escapeHtml(r.userName) + "</td>";
    html += "<td>" + escapeHtml(r.userEmail) + "</td>";
    html += "<td>" + formatDate(r.expectedReturnDate) + "</td>";
    html += "<td><strong>" + r.daysOverdue + "</strong></td>";
    html += "<td>" + items + "</td>";
    html += "</tr>";
  });

  html += "</tbody></table>";
  $("#overdueTable").html(html);
}

function loadDashboard() {
  apiRequest({ url: "/api/reports/dashboard" })
    .done(renderDashboard)
    .fail(function (xhr) {
      showAlert($("#reportAlert"), getErrorMessage(xhr), "error");
    });
}

function loadSummary(fromDate, toDate) {
  let url = "/api/reports/borrow-summary?";
  if (fromDate) url += "fromDate=" + encodeURIComponent(toApiDate(fromDate)) + "&";
  if (toDate) url += "toDate=" + encodeURIComponent(toApiDate(toDate));

  $("#borrowSummary").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i></p>');
  apiRequest({ url: url })
    .done(renderSummary)
    .fail(function (xhr) {
      $("#borrowSummary").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

function loadOverdue() {
  $("#overdueTable").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i></p>');
  apiRequest({ url: "/api/reports/overdue-requests" })
    .done(renderOverdue)
    .fail(function (xhr) {
      $("#overdueTable").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

$(document).ready(function () {
  if (!requireStaffOrAdmin()) return;
  renderAuthNav("reports");

  loadDashboard();
  loadOverdue();
  loadSummary("", "");

  $("#summaryForm").on("submit", function (e) {
    e.preventDefault();
    loadSummary($("#fromDate").val(), $("#toDate").val());
  });
});
