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
        <a href="equipment.html" class="${activePage === "equipment" ? "active" : ""}">
          <i class="fa-solid fa-toolbox"></i> Equipment
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
