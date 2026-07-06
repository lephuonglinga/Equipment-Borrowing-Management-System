let auditPage = 1;

function fetchAuditLogs(page) {
  auditPage = page || 1;
  const params = { pageNumber: auditPage, pageSize: 15 };
  const entityName = $("#entityName").val().trim();
  const action = $("#action").val().trim();
  if (entityName) params.entityName = entityName;
  if (action) params.action = action;

  $("#auditTableWrap").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i></p>');
  apiRequest({ url: "/api/audit-logs?" + $.param(params) })
    .done(function (data) {
      renderAuditTable(data);
      bindPager($("#auditPager"), data, fetchAuditLogs);
    })
    .fail(function (xhr) {
      $("#auditTableWrap").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

function renderAuditTable(data) {
  if (!data.items || data.items.length === 0) {
    $("#auditTableWrap").html('<p class="empty-state">Không có audit log.</p>');
    return;
  }

  let html = '<table class="data-table"><thead><tr>';
  html += "<th>Thời gian</th><th>User</th><th>Entity</th><th>ID</th><th>Action</th><th>Changes</th>";
  html += "</tr></thead><tbody>";

  data.items.forEach(function (log) {
    html += "<tr>";
    html += "<td>" + formatDate(log.performedAt) + "</td>";
    html += "<td>" + escapeHtml(log.userEmail || "—") + "</td>";
    html += "<td>" + escapeHtml(log.entityName) + "</td>";
    html += "<td>" + escapeHtml(log.entityId) + "</td>";
    html += "<td>" + escapeHtml(log.action) + "</td>";
    html += '<td style="max-width:280px;word-break:break-word;">' + escapeHtml(log.changes || "—") + "</td>";
    html += "</tr>";
  });

  html += "</tbody></table>";
  $("#auditTableWrap").html(html);
}

$(document).ready(function () {
  if (!requireAdminRole()) return;
  renderAuthNav("audit");

  fetchAuditLogs(1);

  $("#filterForm").on("submit", function (e) {
    e.preventDefault();
    fetchAuditLogs(1);
  });
});
