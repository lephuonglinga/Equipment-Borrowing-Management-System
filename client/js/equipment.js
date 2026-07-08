const EQUIPMENT_DEFAULT_IMAGE = "images/equipment-default.svg";
let categoryMap = {};

function equipmentImageUrl(item) {
  if (item.imageUrl && item.imageUrl.trim() !== "") {
    return item.imageUrl.trim();
  }
  return EQUIPMENT_DEFAULT_IMAGE;
}

function loadCategories() {
  return apiRequest({ url: "/api/equipment-categories" });
}

function loadEquipment(params) {
  return apiRequest({ url: "/api/equipment?" + $.param(params) });
}

function getUrlCategoryId() {
  const params = new URLSearchParams(window.location.search);
  const id = params.get("categoryId");
  return id ? parseInt(id, 10) : null;
}

function updateCartBar() {
  const count = getBorrowCartCount();
  const $bar = $("#borrowCartBar");
  if (count > 0) {
    $("#cartSummary").text("Đã chọn " + count + " thiết bị");
    $bar.show();
    $("body").addClass("has-cart-bar");
  } else {
    $bar.hide();
    $("body").removeClass("has-cart-bar");
  }
}

function renderBorrowAction(item) {
  if (!isBorrowableEquipment(item)) {
    return "";
  }

  if (isInBorrowCart(item.id)) {
    return (
      '<button type="button" class="btn btn-card-cancel btn-cart-toggle" data-id="' +
      item.id +
      '">Hủy</button>'
    );
  }

  return (
    '<button type="button" class="btn btn-card-borrow btn-cart-toggle" data-id="' +
    item.id +
    '">Mượn</button>'
  );
}

function renderEquipmentCard(item) {
  const imgUrl = equipmentImageUrl(item);
  const isDefault = !item.imageUrl || item.imageUrl.trim() === "";
  const location = item.location ? item.location : "—";
  const onError = isDefault
    ? ""
    : 'onerror="this.onerror=null;this.src=\'' + EQUIPMENT_DEFAULT_IMAGE + "';\"";
  const inCart = isInBorrowCart(item.id) ? " in-cart" : "";
  const catId = $("#categoryId").val();
  let detailHref = "equipment-detail.html?id=" + item.id;
  if (catId) {
    detailHref += "&categoryId=" + encodeURIComponent(catId);
  }

  return (
    '<article class="equipment-card' +
    inCart +
    '" data-id="' +
    item.id +
    '">' +
    '<a class="thumb-link card-link" href="' +
    detailHref +
    '"><div class="thumb"><img src="' +
    imgUrl +
    '" alt="' +
    escapeHtml(item.name) +
    '" loading="lazy" ' +
    onError +
    "></div></a>" +
    '<div class="body">' +
    renderStatusBadge(item.status, "equipment") +
    '<a class="card-link" href="' +
    detailHref +
    '"><h3>' +
    escapeHtml(item.name) +
    "</h3></a>" +
    '<div class="meta"><strong>' +
    escapeHtml(item.serialNumber) +
    "</strong> · " +
    escapeHtml(item.categoryName) +
    "</div>" +
    '<div class="meta"><i class="fa-solid fa-location-dot"></i> ' +
    escapeHtml(location) +
    "</div>" +
    '<div class="card-actions">' +
    renderBorrowAction(item) +
    "</div></div></article>"
  );
}

function renderEquipmentPage(data) {
  const $grid = $("#equipmentGrid");
  const $pager = $("#equipmentPager");

  if (!data.items || data.items.length === 0) {
    $grid.html(
      '<div class="empty-state" style="grid-column:1/-1;"><div><i class="fa-solid fa-box-open"></i></div><p>Không tìm thấy thiết bị phù hợp.</p></div>'
    );
    $pager.hide();
    return;
  }

  $grid.html(data.items.map(renderEquipmentCard).join(""));

  if (data.totalPages > 1) {
    $pager.show();
    $("#pageInfo").text(
      "Trang " + data.pageNumber + " / " + data.totalPages + " (" + data.totalCount + " thiết bị)"
    );
    $("#btnPrev").prop("disabled", !data.hasPrevious);
    $("#btnNext").prop("disabled", !data.hasNext);
  } else {
    $pager.hide();
  }

  $(".btn-cart-toggle").on("click", function () {
    const id = parseInt($(this).data("id"), 10);
    const item = data.items.find(function (eq) {
      return eq.id === id;
    });
    if (!item) return;

    if (isInBorrowCart(id)) {
      removeFromBorrowCart(id);
    } else {
      addToBorrowCart(item);
    }
    fetchEquipment();
  });
}

function getFilterParams() {
  return {
    pageNumber: parseInt($("#pageNumber").val(), 10) || 1,
    pageSize: 8,
    search: $("#search").val().trim(),
    categoryId: $("#categoryId").val() || "",
    status: $("#status").val() || "",
    sortBy: "name",
    sortDirection: "asc"
  };
}

function fetchEquipment() {
  const params = getFilterParams();
  const cleaned = {};
  Object.keys(params).forEach(function (key) {
    if (params[key] !== "" && params[key] != null) {
      cleaned[key] = params[key];
    }
  });

  $("#equipmentGrid").html(
    '<div class="empty-state" style="grid-column:1/-1;"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải...</div>'
  );

  loadEquipment(cleaned)
    .done(function (data) {
      renderEquipmentPage(data);
      const $grid = $("#equipmentGrid");
      if ($grid.length && $grid.offset()) {
        $("html, body").animate({ scrollTop: $grid.offset().top - 80 }, 200);
      }
    })
    .fail(function (xhr) {
      $("#equipmentGrid").html(
        '<div class="empty-state" style="grid-column:1/-1;"><p>' +
          escapeHtml(getErrorMessage(xhr)) +
          "</p></div>"
      );
      $("#equipmentPager").hide();
    });
}

function updatePageTitle() {
  const catId = $("#categoryId").val();
  if (catId && categoryMap[catId]) {
    $("#pageTitle").text("Equipments — " + categoryMap[catId]);
    $("#pageSubtitle").text("Thiết bị thuộc danh mục " + categoryMap[catId] + ".");
  } else {
    $("#pageTitle").text("Equipments");
    $("#pageSubtitle").text("Tất cả thiết bị.");
  }
}

function openConfirmModal() {
  const cart = getBorrowCart();
  if (cart.length === 0) return;

  hideAlert($("#confirmAlert"));
  let listHtml = "";
  cart.forEach(function (item) {
    listHtml +=
      "<li><strong>" +
      escapeHtml(item.name) +
      "</strong> · " +
      escapeHtml(item.serialNumber) +
      "</li>";
  });
  $("#confirmItemList").html(listHtml);
  $("#borrowDate").val(todayInputValue());
  $("#expectedReturnDate").val("");
  $("#purpose").val("");
  $("#confirmBorrowModal").addClass("open");
}

function submitBorrowRequest(e) {
  e.preventDefault();
  hideAlert($("#confirmAlert"));

  const items = borrowCartToApiItems();
  if (items.length === 0) {
    showAlert($("#confirmAlert"), "Chưa chọn thiết bị nào.", "error");
    return;
  }

  $("#btnSubmitBorrow").prop("disabled", true);

  apiRequest({
    url: "/api/borrow-requests",
    method: "POST",
    body: {
      borrowDate: toApiDate($("#borrowDate").val()),
      expectedReturnDate: toApiDate($("#expectedReturnDate").val()),
      purpose: $("#purpose").val().trim(),
      items: items
    }
  })
    .done(function () {
      clearBorrowCart();
      window.location.href = "borrow.html?tab=pending";
    })
    .fail(function (xhr) {
      showAlert($("#confirmAlert"), getErrorMessage(xhr), "error");
    })
    .always(function () {
      $("#btnSubmitBorrow").prop("disabled", false);
    });
}

$(document).ready(function () {
  if (!requireAuth()) return;
  renderAuthNav("equipment");
  updateCartBar();

  const urlCategoryId = getUrlCategoryId();

  loadCategories()
    .done(function (categories) {
      categoryMap = {};
      let options = "";
      categories.forEach(function (c) {
        categoryMap[c.id] = c.name;
        options += '<option value="' + c.id + '">' + escapeHtml(c.name) + "</option>";
      });
      $("#categoryId").append(options);

      if (urlCategoryId && categoryMap[urlCategoryId]) {
        $("#categoryId").val(String(urlCategoryId));
      }
      updatePageTitle();
      fetchEquipment();
    })
    .fail(function () {
      fetchEquipment();
    });

  $("#filterForm").on("submit", function (e) {
    e.preventDefault();
    $("#pageNumber").val(1);
    updatePageTitle();
    fetchEquipment();
  });

  $("#categoryId").on("change", function () {
    $("#pageNumber").val(1);
    updatePageTitle();
    fetchEquipment();
  });

  $("#btnPrev").on("click", function () {
    if ($(this).prop("disabled")) return;
    const page = parseInt($("#pageNumber").val(), 10) || 1;
    if (page > 1) {
      $("#pageNumber").val(page - 1);
      fetchEquipment();
    }
  });

  $("#btnNext").on("click", function () {
    if ($(this).prop("disabled")) return;
    const page = parseInt($("#pageNumber").val(), 10) || 1;
    $("#pageNumber").val(page + 1);
    fetchEquipment();
  });

  $("#btnRegisterBorrow").on("click", openConfirmModal);

  $("#btnClearCart").on("click", function () {
    if (getBorrowCartCount() === 0) return;
    if (!confirm("Bỏ chọn tất cả thiết bị trong danh sách mượn?")) return;
    clearBorrowCart();
    fetchEquipment();
  });
  $("#confirmBorrowForm").on("submit", submitBorrowRequest);

  $(document).on("borrowCartUpdated", function () {
    updateCartBar();
  });

  $("[data-close]").on("click", function () {
    $(this).closest(".modal-overlay").removeClass("open");
  });

  $(".modal-overlay").on("click", function (e) {
    if ($(e.target).hasClass("modal-overlay")) {
      $(this).removeClass("open");
    }
  });
});
