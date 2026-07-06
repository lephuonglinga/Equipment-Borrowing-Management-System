let refreshPromise = null;

function refreshAccessToken() {
  if (refreshPromise) {
    return refreshPromise;
  }

  const auth = typeof getAuth === "function" ? getAuth() : null;
  if (!auth || !auth.refreshToken) {
    return $.Deferred().reject().promise();
  }

  refreshPromise = $.ajax({
    url: EBMS_CONFIG.API_BASE_URL + "/api/auth/refresh",
    type: "POST",
    dataType: "json",
    contentType: "application/json",
    headers: { Accept: "application/json" },
    data: JSON.stringify({ refreshToken: auth.refreshToken })
  })
    .then(function (data) {
      saveAuth(data);
      return data;
    })
    .always(function () {
      refreshPromise = null;
    });

  return refreshPromise;
}

function apiRequest(options) {
  const auth = typeof getAuth === "function" ? getAuth() : null;
  const headers = options.headers || {};

  headers.Accept = "application/json";

  if (auth && auth.accessToken && !options.skipAuthRefresh) {
    headers.Authorization = "Bearer " + auth.accessToken;
  }

  if (options.body && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }

  function sendRequest() {
    const currentAuth = typeof getAuth === "function" ? getAuth() : null;
    if (currentAuth && currentAuth.accessToken && !options.skipAuthRefresh) {
      headers.Authorization = "Bearer " + currentAuth.accessToken;
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

  function shouldTryRefresh(xhr) {
    return (
      xhr &&
      xhr.status === 401 &&
      !options.skipAuthRefresh &&
      !options._authRetried &&
      auth &&
      auth.refreshToken
    );
  }

  const deferred = $.Deferred();

  function runRequest() {
    const currentAuth = typeof getAuth === "function" ? getAuth() : null;
    const startRequest = function () {
      sendRequest()
        .done(deferred.resolve)
        .fail(function (xhr) {
          if (shouldTryRefresh(xhr)) {
            refreshAccessToken()
              .done(function () {
                options._authRetried = true;
                runRequest();
              })
              .fail(function () {
                if (typeof redirectToLoginExpired === "function") {
                  redirectToLoginExpired();
                }
                deferred.reject(xhr);
              });
            return;
          }

          if (
            xhr &&
            xhr.status === 401 &&
            !options.skipAuthRefresh &&
            typeof redirectToLoginExpired === "function"
          ) {
            redirectToLoginExpired();
          }

          deferred.reject(xhr);
        });
    };

    if (
      currentAuth &&
      currentAuth.accessToken &&
      isAccessTokenExpired(currentAuth) &&
      currentAuth.refreshToken &&
      !options.skipAuthRefresh &&
      !options._authRetried
    ) {
      refreshAccessToken()
        .done(startRequest)
        .fail(function () {
          if (typeof redirectToLoginExpired === "function") {
            redirectToLoginExpired();
          }
          deferred.reject();
        });
      return;
    }

    startRequest();
  }

  runRequest();
  return deferred.promise();
}

function getErrorMessage(xhr, context) {
  if (xhr.responseJSON && xhr.responseJSON.message) {
    return xhr.responseJSON.message;
  }
  if (xhr.status === 0) {
    return "Không kết nối được API. Hãy chạy API tại " + EBMS_CONFIG.API_BASE_URL;
  }
  if (xhr.status === 401) {
    if (context === "login") {
      return "Email hoặc mật khẩu không đúng.";
    }
    return "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
  }
  if (xhr.status === 403) {
    return "Tài khoản bị vô hiệu hoặc không có quyền truy cập.";
  }
  return "Đã xảy ra lỗi (HTTP " + xhr.status + ").";
}
