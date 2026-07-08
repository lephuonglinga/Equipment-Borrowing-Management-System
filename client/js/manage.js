let categories = [];
let eqPage = 1;
let eqFilterStatus = "";

function loadCategories() {
  return apiRequest({ url: "/api/equipment-categories" });
}

function buildEquipmentQuery(page) {
  let url =
    "/api/equipment?pageNumber=" +
    (page || 1) +
    "&pageSize=10&sortBy=name&sortDirection=asc";
  if (eqFilterStatus) {
    url += "&status=" + encodeURIComponent(eqFilterStatus);
  }
  return url;
}

function fetchEquipmentPage(page) {
  eqPage = page || 1;
  $("#equipmentTableWrap").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i></p>');
  apiRequest({
    url: buildEquipmentQuery(eqPage)
  })
    .done(function (data) {
      renderEquipmentTable(data);
      bindPager($("#equipmentPager"), data, fetchEquipmentPage);
    })
    .fail(function (xhr) {
      $("#equipmentTableWrap").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

function renderEquipmentTable(data) {
  if (!data.items || data.items.length === 0) {
    $("#equipmentTableWrap").html('<p class="empty-state">Chưa có thiết bị.</p>');
    return;
  }

  let html = '<table class="data-table"><thead><tr>';
  html += "<th>Tên</th><th>Serial</th><th>Danh mục</th><th>Trạng thái</th><th>Vị trí</th><th></th>";
  html += "</tr></thead><tbody>";

  data.items.forEach(function (eq) {
    html += "<tr>";
    html += "<td>" + escapeHtml(eq.name) + "</td>";
    html += "<td>" + escapeHtml(eq.serialNumber) + "</td>";
    html += "<td>" + escapeHtml(eq.categoryName) + "</td>";
    html += "<td>" + renderStatusBadge(eq.status, "equipment") + "</td>";
    html += "<td>" + escapeHtml(eq.location || "—") + "</td>";
    html += '<td class="table-actions">';
    if (eq.status === "Maintenance") {
      html +=
        '<button type="button" class="btn btn-primary btn-sm btn-complete-maint" data-id="' +
        eq.id +
        '">Hoàn tất BT</button> ';
    }
    if (eq.status === "Lost") {
      html +=
        '<button type="button" class="btn btn-danger btn-sm btn-compensate" data-id="' +
        eq.id +
        '">Đã đền bù</button> ';
    }
    if (eq.status !== "Lost" && eq.status !== "Compensated" && eq.status !== "Borrowed" && eq.status !== "Reserved") {
      html += '<button type="button" class="btn btn-ghost btn-sm btn-edit-eq" data-id="' + eq.id + '">Sửa</button>';
      html += '<button type="button" class="btn btn-danger btn-sm btn-del-eq" data-id="' + eq.id + '">Xóa</button>';
    }
    html += "</td></tr>";
  });

  html += "</tbody></table>";
  $("#equipmentTableWrap").html(html);

  $(".btn-edit-eq").on("click", function () {
    openEquipmentForm(parseInt($(this).data("id"), 10));
  });
  $(".btn-del-eq").on("click", function () {
    deleteEquipment(parseInt($(this).data("id"), 10));
  });

  $(".btn-complete-maint").on("click", function () {
    completeMaintenance(parseInt($(this).data("id"), 10));
  });

  $(".btn-compensate").on("click", function () {
    confirmCompensation(parseInt($(this).data("id"), 10));
  });
}

function fillCategorySelect() {
  let opts = "";
  categories.forEach(function (c) {
    opts += '<option value="' + c.id + '">' + escapeHtml(c.name) + "</option>";
  });
  $("#eqCategory").html(opts);
}

function openEquipmentForm(id) {
  hideAlert($("#eqAlert"));
  fillCategorySelect();

  if (id) {
    $("#equipmentModalTitle").text("Sửa thiết bị");
    $("#eqStatusGroup").show();
    apiRequest({ url: "/api/equipment/" + id }).done(function (eq) {
      $("#eqId").val(eq.id);
      $("#eqName").val(eq.name);
      $("#eqSerial").val(eq.serialNumber);
      $("#eqCategory").val(eq.categoryId);
      $("#eqStatus").val(eq.status);
      $("#eqLocation").val(eq.location || "");
      $("#eqDescription").val(eq.description || "");
      $("#eqImageUrl").val(eq.imageUrl || "");
      $("#equipmentModal").addClass("open");
    });
  } else {
    $("#equipmentModalTitle").text("Thêm thiết bị");
    $("#eqId").val("");
    $("#eqStatusGroup").hide();
    $("#equipmentForm")[0].reset();
    $("#equipmentModal").addClass("open");
  }
}

function saveEquipment(e) {
  e.preventDefault();
  hideAlert($("#eqAlert"));

  const id = $("#eqId").val();
  const body = {
    name: $("#eqName").val().trim(),
    serialNumber: $("#eqSerial").val().trim(),
    categoryId: parseInt($("#eqCategory").val(), 10),
    location: $("#eqLocation").val().trim() || null,
    description: $("#eqDescription").val().trim() || null,
    imageUrl: $("#eqImageUrl").val().trim() || null
  };

  let request;
  if (id) {
    body.status = $("#eqStatus").val();
    request = apiRequest({ url: "/api/equipment/" + id, method: "PUT", body: body });
  } else {
    request = apiRequest({ url: "/api/equipment", method: "POST", body: body });
  }

  request
    .done(function () {
      $("#equipmentModal").removeClass("open");
      showAlert($("#pageAlert"), id ? "Đã cập nhật thiết bị." : "Đã thêm thiết bị.", "success");
      fetchEquipmentPage(eqPage);
    })
    .fail(function (xhr) {
      showAlert($("#eqAlert"), getErrorMessage(xhr), "error");
    });
}

function deleteEquipment(id) {
  if (!confirm("Xóa thiết bị này?")) return;
  apiRequest({ url: "/api/equipment/" + id, method: "DELETE" })
    .done(function () {
      showAlert($("#pageAlert"), "Đã xóa thiết bị.", "success");
      fetchEquipmentPage(eqPage);
    })
    .fail(function (xhr) {
      showAlert($("#pageAlert"), getErrorMessage(xhr), "error");
    });
}

function putEquipmentUpdate(id, changes) {
  apiRequest({ url: "/api/equipment/" + id }).done(function (eq) {
    apiRequest({
      url: "/api/equipment/" + id,
      method: "PUT",
      body: {
        name: eq.name,
        serialNumber: eq.serialNumber,
        categoryId: eq.categoryId,
        status: changes.status != null ? changes.status : eq.status,
        location: eq.location,
        description: changes.description != null ? changes.description : eq.description,
        imageUrl: eq.imageUrl
      }
    })
      .done(function () {
        showAlert($("#pageAlert"), changes.message || "Đã cập nhật thiết bị.", "success");
        fetchEquipmentPage(eqPage);
      })
      .fail(function (xhr) {
        showAlert($("#pageAlert"), getErrorMessage(xhr), "error");
      });
  });
}

function completeMaintenance(id) {
  putEquipmentUpdate(id, {
    status: "Available",
    message: "Đã hoàn tất bảo trì."
  });
}

function confirmCompensation(id) {
  if (!confirm("Xác nhận người mượn đã đền bù?")) return;
  putEquipmentUpdate(id, {
    status: "Compensated",
    message: "Đã xác nhận đền bù."
  });
}

function fetchCategories() {
  $("#categoryTableWrap").html('<p class="empty-state"><i class="fa-solid fa-spinner fa-spin"></i></p>');
  loadCategories()
    .done(function (data) {
      categories = data;
      renderCategoryTable(data);
    })
    .fail(function (xhr) {
      $("#categoryTableWrap").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
}

function renderCategoryTable(data) {
  if (!data || data.length === 0) {
    $("#categoryTableWrap").html('<p class="empty-state">Chưa có danh mục.</p>');
    return;
  }

  let html = '<table class="data-table"><thead><tr><th>Tên</th><th>Mô tả</th><th></th></tr></thead><tbody>';
  data.forEach(function (c) {
    html += "<tr>";
    html += "<td>" + escapeHtml(c.name) + "</td>";
    html += "<td>" + escapeHtml(c.description || "—") + "</td>";
    html += '<td class="table-actions">';
    html += '<button type="button" class="btn btn-ghost btn-sm btn-edit-cat" data-id="' + c.id + '">Sửa</button>';
    html += '<button type="button" class="btn btn-danger btn-sm btn-del-cat" data-id="' + c.id + '">Xóa</button>';
    html += "</td></tr>";
  });
  html += "</tbody></table>";
  $("#categoryTableWrap").html(html);

  $(".btn-edit-cat").on("click", function () {
    const id = parseInt($(this).data("id"), 10);
    const cat = categories.find(function (c) {
      return c.id === id;
    });
    if (cat) openCategoryForm(cat);
  });
  $(".btn-del-cat").on("click", function () {
    deleteCategory(parseInt($(this).data("id"), 10));
  });
}

function openCategoryForm(cat) {
  hideAlert($("#catAlert"));
  if (cat) {
    $("#categoryModalTitle").text("Sửa danh mục");
    $("#catId").val(cat.id);
    $("#catName").val(cat.name);
    $("#catDescription").val(cat.description || "");
  } else {
    $("#categoryModalTitle").text("Thêm danh mục");
    $("#catId").val("");
    $("#categoryForm")[0].reset();
  }
  $("#categoryModal").addClass("open");
}

function saveCategory(e) {
  e.preventDefault();
  hideAlert($("#catAlert"));

  const id = $("#catId").val();
  const body = {
    name: $("#catName").val().trim(),
    description: $("#catDescription").val().trim() || null
  };

  const request = id
    ? apiRequest({ url: "/api/equipment-categories/" + id, method: "PUT", body: body })
    : apiRequest({ url: "/api/equipment-categories", method: "POST", body: body });

  request
    .done(function () {
      $("#categoryModal").removeClass("open");
      showAlert($("#pageAlert"), id ? "Đã cập nhật danh mục." : "Đã thêm danh mục.", "success");
      fetchCategories();
    })
    .fail(function (xhr) {
      showAlert($("#catAlert"), getErrorMessage(xhr), "error");
    });
}

function deleteCategory(id) {
  if (!confirm("Xóa danh mục này?")) return;
  apiRequest({ url: "/api/equipment-categories/" + id, method: "DELETE" })
    .done(function () {
      showAlert($("#pageAlert"), "Đã xóa danh mục.", "success");
      fetchCategories();
    })
    .fail(function (xhr) {
      showAlert($("#pageAlert"), getErrorMessage(xhr), "error");
    });
}

$(document).ready(function () {
  if (!requireStaffOrAdmin()) return;
  renderAuthNav("manage");

  fetchCategories();

  $(".tab-btn").on("click", function () {
    const tab = $(this).data("tab");
    $(".tab-btn").removeClass("active");
    $(this).addClass("active");
    $(".tab-panel").removeClass("active");
    $("#tab-" + tab).addClass("active");
  });

  $("#btnAddEquipment").on("click", function () {
    openEquipmentForm(null);
  });
  $("#btnAddCategory").on("click", function () {
    openCategoryForm(null);
  });

  $("#equipmentForm").on("submit", saveEquipment);
  $("#categoryForm").on("submit", saveCategory);

  const params = new URLSearchParams(window.location.search);
  if (params.get("status")) {
    eqFilterStatus = params.get("status");
    $("#eqFilterStatus").val(eqFilterStatus);
  }

  fetchEquipmentPage(1);

  $("#eqFilterStatus").on("change", function () {
    eqFilterStatus = $("#eqFilterStatus").val();
    fetchEquipmentPage(1);
  });

  $("[data-close]").on("click", function () {
    $(this).closest(".modal-overlay").removeClass("open");
  });
});
