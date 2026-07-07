function normalizeUser(u) {
  return {
    id: u.id != null ? u.id : u.Id,
    email: u.email || u.Email || "",
    fullName: u.fullName || u.FullName || "",
    role: u.role || u.Role || "",
    isActive: u.isActive != null ? !!u.isActive : u.IsActive != null ? !!u.IsActive : true,
    createdAt: u.createdAt || u.CreatedAt
  };
}

function isCurrentUser(user) {
  const auth = getAuth();
  if (!auth || !user) return false;
  const authEmail = (auth.email || "").toLowerCase();
  const userEmail = (user.email || "").toLowerCase();
  return authEmail && userEmail && authEmail === userEmail;
}

function fetchUsers() {
  $("#userTableWrap").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i></p>');
  apiRequest({ url: "/api/users" })
    .done(renderUserTable)
    .fail(function (xhr) {
      $("#userTableWrap").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

function renderUserTable(users) {
  if (!users || users.length === 0) {
    $("#userTableWrap").html('<p class="empty-state">Chưa có user.</p>');
    return;
  }

  let html = '<table class="data-table"><thead><tr>';
  html += "<th>ID</th><th>Họ tên</th><th>Email</th><th>Role</th><th>Trạng thái</th><th>Ngày tạo</th><th class=\"col-actions\">Thao tác</th>";
  html += "</tr></thead><tbody>";

  users.map(normalizeUser).forEach(function (u) {
    const isSelf = isCurrentUser(u);
    html += '<tr class="clickable-row" data-href="user-detail.html?id=' + u.id + '">';
    html += "<td>" + u.id + "</td>";
    html += "<td>" + escapeHtml(u.fullName) + "</td>";
    html += "<td>" + escapeHtml(u.email) + "</td>";
    html += "<td>" + escapeHtml(u.role) + "</td>";
    html += "<td>" + (u.isActive ? '<span class="status-badge status-available">Active</span>' : '<span class="status-badge borrow-rejected">Inactive</span>') + "</td>";
    html += "<td>" + formatDate(u.createdAt) + "</td>";
    html += '<td class="table-actions">';
    if (u.isActive) {
      html +=
        '<button type="button" class="btn btn-danger btn-sm btn-deactivate" data-id="' +
        u.id +
        '"' +
        (isSelf ? ' disabled title="Không thể vô hiệu tài khoản đang đăng nhập"' : "") +
        ">Vô hiệu</button>";
    } else {
      html +=
        '<button type="button" class="btn btn-primary btn-sm btn-activate" data-id="' +
        u.id +
        '"' +
        (isSelf ? ' disabled title="Không thể kích hoạt tài khoản đang đăng nhập"' : "") +
        ">Kích hoạt</button>";
    }
    html += "</td></tr>";
  });

  html += "</tbody></table>";
  $("#userTableWrap").html(html);

  $(".clickable-row").on("click", function (e) {
    if ($(e.target).closest("button, a").length) return;
    window.location.href = $(this).data("href");
  });

  $(".btn-deactivate").on("click", function (e) {
    e.stopPropagation();
    if ($(this).prop("disabled")) return;
    toggleUser(parseInt($(this).data("id"), 10), false);
  });
  $(".btn-activate").on("click", function (e) {
    e.stopPropagation();
    if ($(this).prop("disabled")) return;
    toggleUser(parseInt($(this).data("id"), 10), true);
  });
}

function toggleUser(id, activate) {
  const msg = activate ? "Kích hoạt user này?" : "Vô hiệu hóa user này?";
  if (!confirm(msg)) return;

  apiRequest({ url: "/api/users/" + id, method: "PATCH", body: { isActive: activate } })
    .done(function () {
      showAlert($("#userAlert"), activate ? "Đã kích hoạt user." : "Đã vô hiệu hóa user.", "success");
      fetchUsers();
    })
    .fail(function (xhr) {
      showAlert($("#userAlert"), getErrorMessage(xhr), "error");
    });
}

function createUser(e) {
  e.preventDefault();
  hideAlert($("#userFormAlert"));

  apiRequest({
    url: "/api/users",
    method: "POST",
    body: {
      fullName: $("#userFullName").val().trim(),
      email: $("#userEmail").val().trim(),
      password: $("#userPassword").val()
    }
  })
    .done(function () {
      $("#userModal").removeClass("open");
      $("#userForm")[0].reset();
      showAlert($("#userAlert"), "Đã tạo tài khoản Staff.", "success");
      fetchUsers();
    })
    .fail(function (xhr) {
      showAlert($("#userFormAlert"), getErrorMessage(xhr), "error");
    });
}

$(document).ready(function () {
  if (!requireAdminRole()) return;
  renderAuthNav("users");

  fetchUsers();

  $("#btnAddUser").on("click", function () {
    hideAlert($("#userFormAlert"));
    $("#userForm")[0].reset();
    $("#userModal").addClass("open");
  });

  $("#userForm").on("submit", createUser);

  $("[data-close]").on("click", function () {
    $(this).closest(".modal-overlay").removeClass("open");
  });
});
