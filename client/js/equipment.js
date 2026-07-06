const EQUIPMENT_DEFAULT_IMAGE = "images/equipment-default.svg";

function equipmentImageUrl(item) {
  if (item.imageUrl && item.imageUrl.trim() !== "") {
    return item.imageUrl.trim();
  }
  return EQUIPMENT_DEFAULT_IMAGE;
}

function statusClass(status) {
  return "status-" + (status || "available").toLowerCase();
}

function loadCategories() {
  return apiRequest({ url: "/api/equipment-categories" });
}

function loadEquipment(params) {
  const query = $.param(params);
  return apiRequest({ url: "/api/equipment?" + query });
}

function renderEquipmentCard(item) {
  const imgUrl = equipmentImageUrl(item);
  const isDefault = !item.imageUrl || item.imageUrl.trim() === "";
  const location = item.location ? item.location : "—";
  const onError = isDefault
    ? ""
    : `onerror="this.onerror=null;this.src='${EQUIPMENT_DEFAULT_IMAGE}';"`;

  return `
    <article class="equipment-card">
      <div class="thumb">
        <img src="${imgUrl}" alt="${item.name}" loading="lazy" ${onError}>
      </div>
      <div class="body">
        <span class="status-badge ${statusClass(item.status)}">${item.status}</span>
        <h3>${item.name}</h3>
        <div class="meta"><strong>${item.serialNumber}</strong> · ${item.categoryName}</div>
        <div class="meta"><i class="fa-solid fa-location-dot"></i> ${location}</div>
      </div>
    </article>
  `;
}

function renderEquipmentPage(data) {
  const $grid = $("#equipmentGrid");
  const $pager = $("#equipmentPager");

  if (!data.items || data.items.length === 0) {
    $grid.html(`
      <div class="empty-state" style="grid-column:1/-1;">
        <div><i class="fa-solid fa-box-open"></i></div>
        <p>Không tìm thấy thiết bị phù hợp.</p>
      </div>
    `);
    $pager.hide();
    return;
  }

  const html = data.items.map(renderEquipmentCard).join("");
  $grid.html(html);

  $pager.show();
  $("#pageInfo").text(`Trang ${data.pageNumber} / ${data.totalPages} (${data.totalCount} thiết bị)`);
  $("#btnPrev").prop("disabled", !data.hasPrevious);
  $("#btnNext").prop("disabled", !data.hasNext);
}

function getFilterParams() {
  return {
    pageNumber: parseInt($("#pageNumber").val(), 10) || 1,
    pageSize: 12,
    search: $("#search").val().trim(),
    categoryId: $("#categoryId").val() || "",
    status: $("#status").val() || "",
    sortBy: $("#sortBy").val() || "name",
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

  $("#equipmentGrid").html('<div class="empty-state" style="grid-column:1/-1;"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải...</div>');

  loadEquipment(cleaned)
    .done(function (data) {
      renderEquipmentPage(data);
    })
    .fail(function (xhr) {
      $("#equipmentGrid").html(`
        <div class="empty-state" style="grid-column:1/-1;">
          <p>${getErrorMessage(xhr)}</p>
        </div>
      `);
      $("#equipmentPager").hide();
    });
}

$(document).ready(function () {
  if (!requireAuth()) return;

  renderAuthNav("equipment");

  loadCategories()
    .done(function (categories) {
      const options = categories.map(function (c) {
        return `<option value="${c.id}">${c.name}</option>`;
      }).join("");
      $("#categoryId").append(options);
    })
    .always(function () {
      fetchEquipment();
    });

  $("#filterForm").on("submit", function (e) {
    e.preventDefault();
    $("#pageNumber").val(1);
    fetchEquipment();
  });

  $("#btnPrev").on("click", function () {
    const page = parseInt($("#pageNumber").val(), 10) || 1;
    if (page > 1) {
      $("#pageNumber").val(page - 1);
      fetchEquipment();
    }
  });

  $("#btnNext").on("click", function () {
    const page = parseInt($("#pageNumber").val(), 10) || 1;
    $("#pageNumber").val(page + 1);
    fetchEquipment();
  });
});
