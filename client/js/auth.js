const AUTH_STORAGE_KEY = "ebms_auth";

function saveAuth(data) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    expiresAt: data.expiresAt,
    email: data.email,
    fullName: data.fullName,
    role: data.role
  }));
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

function requireAuth() {
  if (!getAuth()) {
    window.location.href = "login.html";
    return false;
  }
  return true;
}

function logout() {
  const auth = getAuth();
  if (auth && auth.refreshToken) {
    apiRequest({
      url: "/api/auth/logout",
      method: "POST",
      body: { refreshToken: auth.refreshToken }
    }).always(function () {
      clearAuth();
      window.location.href = "login.html";
    });
  } else {
    clearAuth();
    window.location.href = "login.html";
  }
}

function getRoleIcon(role) {
  if (role === "Admin") return "fa-user-shield";
  if (role === "Staff") return "fa-user-tie";
  return "fa-user";
}

function renderAuthNav(activePage) {
  const auth = getAuth();
  const roleIcon = auth ? getRoleIcon(auth.role) : "fa-user";

  const html = `
    <header class="site-header">
      <div class="brand">
        <i class="fa-solid fa-toolbox"></i>
        <span>EBMS</span>
      </div>
      <nav>
        <a href="home.html" class="${activePage === "home" ? "active" : ""}">
          <i class="fa-solid fa-house"></i> Trang chủ
        </a>
        <a href="notifications.html" class="${activePage === "notifications" ? "active" : ""}">
          <i class="fa-regular fa-bell"></i> Notifications
        </a>
        <span class="nav-role">
          <i class="fa-solid ${roleIcon}"></i> ${auth ? auth.role : ""}
        </span>
      </nav>
    </header>
  `;

  $("#siteHeader").html(html);
}
