let currentUser = null;

function getUserId() {
  const id = new URLSearchParams(window.location.search).get("id");
  return id ? parseInt(id, 10) : null;
}

function isSelf(user) {
  const auth = getAuth();
  return auth && user && auth.email === user.email;
}

function renderDetail(user) {
  currentUser = user;
  const self = isSelf(user);
  const statusBadge = user.isActive
    ? '<span class="status-badge status-available">Active</span>'
    : '<span class="status-badge borrow-rejected">Inactive</span>';

  let html = '<div class="user-detail">';
  html += "<h1>" + escapeHtml(user.fullName) + "</h1>";
  html += '<div class="detail-grid">';
  html += '<div><div class="label">ID</div>' + user.id + "</div>";
  html += '<div><div class="label">Role</div>' + escapeHtml(user.role) + "</div>";
  html += '<div><div class="label">Email</div>' + escapeHtml(user.email) + "</div>";
  html += '<div><div class="label">Trạng thái</div>' + statusBadge + "</div>";
  html += '<div><div class="label">Ngày tạo</div>' + formatDate(user.createdAt) + "</div>";
  html += "</div>";

  html += '<div class="detail-actions">';
  if (self) {
    html += '<p class="detail-note">Đây là tài khoản đang đăng nhập — không thể tự kích hoạt/vô hiệu.</p>';
  } else if (user.isActive) {
    html +=
      '<button type="button" class="btn btn-danger btn-fit" id="btnDeactivate"><i class="fa-solid fa-user-slash"></i> Vô hiệu hóa</button>';
  } else {
    html +=
      '<button type="button" class="btn btn-primary btn-fit" id="btnActivate"><i class="fa-solid fa-user-check"></i> Kích hoạt</button>';
  }
  html += "</div></div>";

  $("#detailContent").html(html);

  $("#btnDeactivate").on("click", function () {
    toggleUser(false);
  });
  $("#btnActivate").on("click", function () {
    toggleUser(true);
  });
}

function toggleUser(activate) {
  if (!currentUser) return;
  const action = activate ? "activate" : "deactivate";
  const msg = activate ? "Kích hoạt user này?" : "Vô hiệu hóa user này?";
  if (!confirm(msg)) return;

  apiRequest({ url: "/api/users/" + currentUser.id + "/" + action, method: "PUT" })
    .done(function (data) {
      showAlert($("#detailAlert"), activate ? "Đã kích hoạt user." : "Đã vô hiệu hóa user.", "success");
      renderDetail(data);
    })
    .fail(function (xhr) {
      showAlert($("#detailAlert"), getErrorMessage(xhr), "error");
    });
}

function loadDetail() {
  const id = getUserId();
  if (!id) {
    $("#detailContent").html('<p class="empty-state">Không tìm thấy user.</p>');
    return;
  }

  apiRequest({ url: "/api/users/" + id })
    .done(renderDetail)
    .fail(function (xhr) {
      $("#detailContent").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

$(document).ready(function () {
  if (!requireAdminRole()) return;
  renderAuthNav("users");
  loadDetail();
});
