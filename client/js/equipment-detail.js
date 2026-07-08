const EQUIPMENT_DEFAULT_IMAGE = "images/equipment-default.svg";
let currentEquipment = null;

function getEquipmentId() {
  const id = new URLSearchParams(window.location.search).get("id");
  return id ? parseInt(id, 10) : null;
}

function equipmentImageUrl(item) {
  if (item.imageUrl && item.imageUrl.trim() !== "") {
    return item.imageUrl.trim();
  }
  return EQUIPMENT_DEFAULT_IMAGE;
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

function renderBorrowButton(item) {
  if (!isBorrowableEquipment(item)) {
    return '<p class="detail-note">Thiết bị không khả dụng để mượn (kiểm tra trạng thái).</p>';
  }
  let html = "";
  if (isInBorrowCart(item.id)) {
    html += '<button type="button" class="btn btn-card-cancel" id="btnCartToggle">Hủy chọn mượn</button>';
  } else {
    html += '<button type="button" class="btn btn-card-borrow" id="btnCartToggle">Mượn</button>';
  }
  return html;
}

function renderDetail(item) {
  currentEquipment = item;
  const imgUrl = equipmentImageUrl(item);
  const isDefault = !item.imageUrl || item.imageUrl.trim() === "";
  const onError = isDefault
    ? ""
    : 'onerror="this.onerror=null;this.src=\'' + EQUIPMENT_DEFAULT_IMAGE + "';\"";

  let html = '<div class="equipment-detail">';
  html += '<div class="equipment-detail-image">';
  html += '<img src="' + imgUrl + '" alt="' + escapeHtml(item.name) + '" ' + onError + ">";
  html += "</div>";
  html += '<div class="equipment-detail-body">';
  html += renderStatusBadge(item.status, "equipment");
  html += "<h1>" + escapeHtml(item.name) + "</h1>";
  html += '<div class="detail-grid">';
  html += '<div><div class="label">Serial</div>' + escapeHtml(item.serialNumber) + "</div>";
  html += '<div><div class="label">Danh mục</div>' + escapeHtml(item.categoryName) + "</div>";
  html += '<div><div class="label">Vị trí</div>' + escapeHtml(item.location || "—") + "</div>";
  html += '<div class="full"><div class="label">Mô tả</div>' + escapeHtml(item.description || "—") + "</div>";
  html += "</div>";
  html += '<div class="detail-actions">' + renderBorrowButton(item) + "</div>";
  html += "</div></div>";

  $("#detailContent").html(html);

  $("#btnCartToggle").on("click", function () {
    if (!currentEquipment) return;
    if (isInBorrowCart(currentEquipment.id)) {
      removeFromBorrowCart(currentEquipment.id);
    } else {
      addToBorrowCart(currentEquipment);
    }
    renderDetail(currentEquipment);
    updateCartBar();
  });
}

function loadDetail() {
  const id = getEquipmentId();
  if (!id) {
    $("#detailContent").html('<p class="empty-state">Không tìm thấy thiết bị.</p>');
    return;
  }

  apiRequest({ url: "/api/equipment/" + id })
    .done(renderDetail)
    .fail(function (xhr) {
      $("#detailContent").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
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

$(document).ready(function () {
  if (!requireAuth()) return;
  renderAuthNav("equipment");
  updateCartBar();

  const returnCat = new URLSearchParams(window.location.search).get("categoryId");
  if (returnCat) {
    $("#backLink").attr("href", "equipment.html?categoryId=" + encodeURIComponent(returnCat));
  }

  loadDetail();

  $("#btnRegisterBorrow").on("click", openConfirmModal);
  $("#btnClearCart").on("click", function () {
    if (getBorrowCartCount() === 0) return;
    if (!confirm("Bỏ chọn tất cả thiết bị trong danh sách mượn?")) return;
    clearBorrowCart();
    if (currentEquipment) renderDetail(currentEquipment);
    updateCartBar();
  });

  $("#confirmBorrowForm").on("submit", function (e) {
    e.preventDefault();
    hideAlert($("#confirmAlert"));
    const items = borrowCartToApiItems();
    if (items.length === 0) {
      showAlert($("#confirmAlert"), "Chưa chọn thiết bị nào.", "error");
      return;
    }
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
      });
  });

  $(document).on("borrowCartUpdated", updateCartBar);

  $("[data-close]").on("click", function () {
    $(this).closest(".modal-overlay").removeClass("open");
  });
});
