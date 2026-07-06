const AUTH_STORAGE_KEY = "ebms_auth";

function normalizeAuthData(data) {
  return {
    accessToken: data.accessToken || data.AccessToken || "",
    refreshToken: data.refreshToken || data.RefreshToken || "",
    expiresAt: data.expiresAt || data.ExpiresAt || "",
    email: data.email || data.Email || "",
    fullName: data.fullName || data.FullName || "",
    role: data.role || data.Role || ""
  };
}

function saveAuth(data) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(normalizeAuthData(data)));
}

function isAccessTokenExpired(auth) {
  if (!auth || !auth.expiresAt) return false;
  const expiresAt = new Date(auth.expiresAt).getTime();
  if (Number.isNaN(expiresAt)) return false;
  return Date.now() >= expiresAt - 30000;
}

function getAuth() {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

function clearAuth() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

function isStaffOrAdmin() {
  const auth = getAuth();
  return auth && (auth.role === "Staff" || auth.role === "Admin");
}

function isAdmin() {
  const auth = getAuth();
  return auth && auth.role === "Admin";
}

function requireAuth() {
  const auth = getAuth();
  if (!auth || !auth.accessToken) {
    clearAuth();
    window.location.href = "login.html";
    return false;
  }
  return true;
}

function redirectToLoginExpired() {
  clearAuth();
  window.location.href = "login.html?expired=1";
}

function logout() {
  const auth = getAuth();
  if (auth && auth.refreshToken) {
    apiRequest({
      url: "/api/auth/logout",
      method: "POST",
      skipAuthRefresh: true,
      expectEmpty: true,
      body: { refreshToken: auth.refreshToken }
    }).always(function () {
      clearAuth();
      clearBorrowCart();
      window.location.href = "login.html";
    });
  } else {
    clearAuth();
    if (typeof clearBorrowCart === "function") clearBorrowCart();
    window.location.href = "login.html";
  }
}

function getRoleIcon(role) {
  if (role === "Admin") return "fa-user-shield";
  if (role === "Staff") return "fa-user-tie";
  return "fa-user";
}

function bindUserMenu() {
  const $menu = $("#navUserMenu");
  const $btn = $("#navUserBtn");

  $btn.on("click", function (e) {
    e.stopPropagation();
    $menu.toggleClass("open");
  });

  $(document).on("click", function () {
    $menu.removeClass("open");
  });

  $("#btnNavLogout").on("click", function (e) {
    e.stopPropagation();
    logout();
  });
}

function renderAuthNav(activePage) {
  const auth = getAuth();
  const roleIcon = auth ? getRoleIcon(auth.role) : "fa-user";
  const staff = isStaffOrAdmin();
  const admin = isAdmin();

  let staffLinks = "";
  if (staff) {
    staffLinks +=
      '<a href="reports.html" class="nav-staff ' + (activePage === "reports" ? "active" : "") + '"><i class="fa-solid fa-chart-pie"></i> Báo cáo</a>';
    staffLinks +=
      '<a href="manage.html" class="nav-staff ' + (activePage === "manage" ? "active" : "") + '"><i class="fa-solid fa-pen-to-square"></i> Quản lý</a>';
  }
  if (admin) {
    staffLinks +=
      '<a href="users.html" class="nav-staff ' + (activePage === "users" ? "active" : "") + '"><i class="fa-solid fa-users"></i> Users</a>';
    staffLinks +=
      '<a href="audit-logs.html" class="nav-staff ' + (activePage === "audit" ? "active" : "") + '"><i class="fa-solid fa-list-check"></i> Audit</a>';
  }

  const displayName = auth && auth.fullName ? auth.fullName.split(" ")[0] : "User";

  const html =
    '<header class="site-header">' +
    '<div class="brand"><a href="categories.html"><i class="fa-solid fa-toolbox"></i><span>EBMS</span></a></div>' +
    "<nav>" +
    '<a href="categories.html" class="' + (activePage === "categories" ? "active" : "") + '">Categories</a>' +
    '<a href="equipment.html" class="' + (activePage === "equipment" ? "active" : "") + '">Equipments</a>' +
    '<a href="borrow.html" class="' + (activePage === "borrow" ? "active" : "") + '">' +
    (staff ? "Duyệt mượn" : "Yêu cầu mượn") +
    "</a>" +
    staffLinks +
    '<a href="notifications.html" class="nav-bell ' + (activePage === "notifications" ? "active" : "") + '" title="Thông báo">' +
    '<i class="fa-regular fa-bell"></i></a>' +
    '<div class="nav-user-menu" id="navUserMenu">' +
    '<button type="button" class="nav-user-btn" id="navUserBtn">' +
    '<i class="fa-solid ' + roleIcon + '"></i> ' + escapeHtml(displayName) + ' <i class="fa-solid fa-chevron-down"></i>' +
    "</button>" +
    '<div class="nav-user-dropdown">' +
    '<button type="button" id="btnNavLogout"><i class="fa-solid fa-right-from-bracket"></i> Đăng xuất</button>' +
    "</div></div>" +
    "</nav></header>";

  $("#siteHeader").html(html);
  bindUserMenu();
}
