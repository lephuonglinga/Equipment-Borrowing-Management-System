function escapeHtml(text) {
  if (text == null) return "";
  return String(text)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function formatDate(value) {
  if (!value) return "—";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return escapeHtml(value);
  return d.toLocaleDateString("vi-VN");
}

function toApiDate(dateStr) {
  if (!dateStr) return "";
  return dateStr + "T00:00:00.000Z";
}

function todayInputValue() {
  return new Date().toISOString().slice(0, 10);
}

function showAlert($el, message, type) {
  const css = type === "success" ? "alert alert-success" : "alert alert-error";
  const icon = type === "success" ? "fa-circle-check" : "fa-circle-exclamation";
  $el
    .removeClass("alert-success alert-error")
    .addClass(css)
    .html('<i class="fa-solid ' + icon + '"></i><span>' + escapeHtml(message) + "</span>")
    .show();
}

function hideAlert($el) {
  $el.hide().empty();
}

function equipmentStatusClass(status) {
  return "status-" + (status || "available").toLowerCase();
}

function borrowStatusClass(status) {
  const map = {
    Pending: "borrow-pending",
    Approved: "borrow-approved",
    Rejected: "borrow-rejected",
    Cancelled: "borrow-cancelled",
    InProgress: "borrow-progress",
    Returned: "borrow-returned",
    Completed: "borrow-completed",
    Overdue: "borrow-overdue"
  };
  return map[status] || "borrow-default";
}

function renderStatusBadge(status, type) {
  const cls = type === "borrow" ? borrowStatusClass(status) : equipmentStatusClass(status);
  return '<span class="status-badge ' + cls + '">' + escapeHtml(status) + "</span>";
}

function requireStaffOrAdmin() {
  if (!requireAuth()) return false;
  if (!isStaffOrAdmin()) {
    window.location.href = "categories.html";
    return false;
  }
  return true;
}

function requireAdminRole() {
  if (!requireAuth()) return false;
  if (!isAdmin()) {
    window.location.href = "categories.html";
    return false;
  }
  return true;
}

function renderPager(data, onPage) {
  if (!data || data.totalPages <= 1) {
    return "";
  }

  let html = '<div class="table-pager">';
  html += '<button type="button" class="btn btn-ghost btn-sm" data-page="' + (data.pageNumber - 1) + '" ' + (data.hasPrevious ? "" : "disabled") + ">Trước</button>";
  html += '<span>Trang ' + data.pageNumber + " / " + data.totalPages + " (" + data.totalCount + ")</span>";
  html += '<button type="button" class="btn btn-ghost btn-sm" data-page="' + (data.pageNumber + 1) + '" ' + (data.hasNext ? "" : "disabled") + ">Sau</button>";
  html += "</div>";

  return html;
}

function bindPager($container, data, onPage) {
  $container.html(renderPager(data, onPage));
  $container.find("button[data-page]").on("click", function () {
    if ($(this).prop("disabled")) return;
    onPage(parseInt($(this).data("page"), 10));
  });
}
