function apiRequest(options) {
  const auth = typeof getAuth === "function" ? getAuth() : null;
  const headers = options.headers || {};

  if (auth && auth.accessToken) {
    headers["Authorization"] = "Bearer " + auth.accessToken;
  }

  if (options.body && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }

  return $.ajax({
    url: EBMS_CONFIG.API_BASE_URL + options.url,
    type: options.method || "GET",
    dataType: "json",
    contentType: options.body ? "application/json" : undefined,
    data: options.body ? JSON.stringify(options.body) : undefined,
    headers: headers
  });
}

function getErrorMessage(xhr) {
  if (xhr.responseJSON && xhr.responseJSON.message) {
    return xhr.responseJSON.message;
  }
  if (xhr.status === 0) {
    return "Không kết nối được API. Hãy chạy API tại " + EBMS_CONFIG.API_BASE_URL;
  }
  if (xhr.status === 401) {
    return "Email hoặc mật khẩu không đúng.";
  }
  if (xhr.status === 403) {
    return "Tài khoản bị vô hiệu hoặc không có quyền truy cập.";
  }
  return "Đã xảy ra lỗi (HTTP " + xhr.status + ").";
}
