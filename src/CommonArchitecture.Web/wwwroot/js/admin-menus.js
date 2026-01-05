// Menus Index Page - JavaScript Module
// Handles CRUD operations, pagination, sorting, and search functionality

$(document).ready(function () {
 // State variables
 let currentMenuId = null;
 let isEditMode = false;
 let currentPage = 1;
 let pageSize = 10;
 let sortBy = 'DisplayOrder';
 let sortOrder = 'asc';
 let searchTerm = '';
 let searchTimeout = null;
 const token = $('input[name="__RequestVerificationToken"]').val();

 // Initialize
 loadParentMenus();
 loadMenus();
 initializeValidation();
 attachEventHandlers();

 // ========================================
 // Validation Setup
 // ========================================
 function initializeValidation() {
 $("#menuForm").validate({
 rules: {
 Name: { required: true, maxlength: 128 },
 Url: { required: true, maxlength: 256 },
 Icon: { maxlength: 64 },
 DisplayOrder: { required: true, min: 0 }
 },
 errorPlacement: function (error, element) {
 $(`#error-${element.attr('name')}`).html(error);
 },
 success: function (label, element) {
 $(`#error-${$(element).attr('name')}`).html('');
 }
 });
 }

 // ========================================
 // Event Handlers
 // ========================================
 function attachEventHandlers() {
 // Search functionality with debounce
 $('#searchBox').on('input', function () {
 clearTimeout(searchTimeout);
 searchTimeout = setTimeout(function () {
 searchTerm = $('#searchBox').val();
 currentPage = 1;
 loadMenus();
 }, 300);
 });

 // Clear search
 $('#btnClearSearch').click(function () {
 $('#searchBox').val('');
 searchTerm = '';
 currentPage = 1;
 loadMenus();
 });

 // Page size change
 $('#pageSizeSelect').change(function () {
 pageSize = parseInt($(this).val());
 currentPage = 1;
 loadMenus();
 });

 // Column sorting
 $(document).on('click', '.sortable', function () {
 const column = $(this).data('column');
 if (sortBy === column) {
 sortOrder = sortOrder === 'asc' ? 'desc' : 'asc';
 } else {
 sortBy = column;
 sortOrder = 'asc';
 }
 updateSortIndicators();
 loadMenus();
 });

 // Pagination clicks
 $(document).on('click', '#pagination a', function (e) {
 e.preventDefault();
 const page = parseInt($(this).data('page'));
 if (page && page !== currentPage) {
 currentPage = page;
 loadMenus();
 }
 });

 // Create menu button
 $('#btnCreateMenu').click(function () {
 isEditMode = false;
 currentMenuId = null;
 $('#menuModalLabel').text('Create Menu');
 $('#saveButtonText').text('Save Menu');
 resetForm();
 $('#menuModal').modal('show');
 });

 // Form submission
 $('#menuForm').submit(function (e) {
 e.preventDefault();
 if (!$(this).valid()) return;
 saveMenu();
 });

 // Confirm delete
 $('#btnConfirmDelete').click(function () {
 deleteMenu();
 });

 // Reset form on modal close
 $('#menuModal').on('hidden.bs.modal', resetForm);
 }

 // ========================================
 // Sorting Functions
 // ========================================
 function updateSortIndicators() {
 $('.sortable').removeClass('active');
 $('.sortable i').removeClass('fa-arrow-up fa-arrow-down').addClass('fa-arrow-down-up');
 const activeHeader = $(`.sortable[data-column="${sortBy}"]`);
 activeHeader.addClass('active');
 activeHeader.find('i').removeClass('fa-arrow-down-up')
 .addClass(sortOrder === 'asc' ? 'fa-arrow-up' : 'fa-arrow-down');
 }

 // ========================================
 // Data Loading Functions
 // ========================================
 function loadMenus() {
 $('#loadingIndicator').show();
 $('#menusTable').hide();
 $('#emptyState').hide();

 const params = {
 PageNumber: currentPage,
 PageSize: pageSize,
 SortBy: sortBy,
 SortOrder: sortOrder
 };

 if (searchTerm) params.SearchTerm = searchTerm;

 $.ajax({
 url: '/Admin/Menus/GetAll',
 type: 'GET',
 data: params,
 success: function (response) {
 $('#loadingIndicator').hide();
 if (response.success) {
 renderMenus(response.data);
 }
 },
 error: function () {
 $('#loadingIndicator').hide();
 showAlert('danger', 'Error loading menus. Please try again.');
 }
 });
 }

 function loadParentMenus() {
 $.ajax({
 url: '/Admin/Menus/GetAll?PageSize=100',
 type: 'GET',
 success: function (response) {
 if (response.success && response.data.items) {
 const parentSelect = $('#menuParentId');
 response.data.items.forEach(function (menu) {
 if (!menu.parentMenuId) { // Only top-level menus
 parentSelect.append(`<option value="${menu.id}">${menu.name}</option>`);
 }
 });
 }
 },
 error: function () {
 // Non-critical, continue without parent menu options
 }
 });
 }

 // ========================================
 // Rendering Functions
 // ========================================
 function renderMenus(result) {
 const tbody = $('#menusTableBody');
 tbody.empty();

 if (result.items.length === 0) {
 $('#menusTable').hide();
 $('#emptyState').show();
 $('#pagination').empty();
 $('#resultInfo').text('');
 return;
 }

 $('#menusTable').show();
 $('#emptyState').hide();

 // Call the inline function from the view
 if (window.renderMenusTable) {
 window.renderMenusTable(result.items);
 }

 renderPagination(result);
 updateResultInfo(result);
 }

 function renderPagination(result) {
 const pagination = $('#pagination');
 pagination.empty();

 if (result.totalPages <= 1) return;

 const prevDisabled = !result.hasPrevious ? 'disabled' : '';
 pagination.append(`
 <li class="page-item ${prevDisabled}">
 <a class="page-link" href="#" data-page="${result.pageNumber - 1}">Previous</a>
 </li>
 `);

 const startPage = Math.max(1, result.pageNumber - 2);
 const endPage = Math.min(result.totalPages, result.pageNumber + 2);

 for (let i = startPage; i <= endPage; i++) {
 const active = i === result.pageNumber ? 'active' : '';
 pagination.append(`
 <li class="page-item ${active}">
 <a class="page-link" href="#" data-page="${i}">${i}</a>
 </li>
 `);
 }

 const nextDisabled = !result.hasNext ? 'disabled' : '';
 pagination.append(`
 <li class="page-item ${nextDisabled}">
 <a class="page-link" href="#" data-page="${result.pageNumber + 1}">Next</a>
 </li>
 `);
 }

 function updateResultInfo(result) {
 const start = (result.pageNumber - 1) * result.pageSize + 1;
 const end = Math.min(result.pageNumber * result.pageSize, result.totalCount);
 $('#resultInfo').text(`Showing ${start}-${end} of ${result.totalCount} menus`);
 }

 // ========================================
 // CRUD Operations (Global Functions)
 // ========================================
 window.editMenu = function(menuId) {
 isEditMode = true;
 currentMenuId = menuId;
 $('#menuModalLabel').text('Edit Menu');
 $('#saveButtonText').text('Update Menu');

 $.ajax({
 url: `/Admin/Menus/GetById/${menuId}`,
 type: 'GET',
 success: function (response) {
 if (response.success) {
 const menu = response.data;
 $('#menuId').val(menu.id);
 $('#menuName').val(menu.name);
 $('#menuUrl').val(menu.url);
 $('#menuIcon').val(menu.icon);
 $('#menuDisplayOrder').val(menu.displayOrder);
 $('#menuParentId').val(menu.parentMenuId || '');
 $('#menuIsActive').prop('checked', menu.isActive);
 $('#menuModal').modal('show');
 } else {
 showAlert('danger', 'Failed to load menu details.');
 }
 },
 error: function () {
 showAlert('danger', 'Error loading menu details. Please try again.');
 }
 });
 };

 window.deleteMenu = function(menuId, menuName) {
 currentMenuId = menuId;
 $('#deleteMenuName').text(menuName);
 $('#deleteModal').modal('show');
 };

 function saveMenu() {
 const formData = {
 Name: $('#menuName').val(),
 Icon: $('#menuIcon').val(),
 Url: $('#menuUrl').val(),
 ParentMenuId: $('#menuParentId').val() ? parseInt($('#menuParentId').val()) : null,
 DisplayOrder: parseInt($('#menuDisplayOrder').val()),
 IsActive: $('#menuIsActive').is(':checked')
 };

 const url = isEditMode
 ? `/Admin/Menus/Edit/${currentMenuId}`
 : '/Admin/Menus/Create';

 $('#saveSpinner').removeClass('d-none');
 $('#btnSaveMenu').prop('disabled', true);

 $.ajax({
 url: url,
 type: isEditMode ? 'PUT' : 'POST',
 contentType: 'application/json',
 data: JSON.stringify(formData),
 headers: { 'RequestVerificationToken': token },
 success: function (response) {
 if (response.success) {
 $('#menuModal').modal('hide');
 showAlert('success', response.message);
 loadMenus();
 } else if (response.errors) {
 for (const [field, messages] of Object.entries(response.errors)) {
 $(`#error-${field}`).html(messages.join('<br>'));
 }
 } else {
 showAlert('danger', response.message || 'An error occurred.');
 }
 },
 error: function () {
 showAlert('danger', 'An error occurred while saving the menu. Please try again.');
 },
 complete: function () {
 $('#saveSpinner').addClass('d-none');
 $('#btnSaveMenu').prop('disabled', false);
 }
 });
 }

 function deleteMenu() {
 $('#deleteSpinner').removeClass('d-none');
 $('#btnConfirmDelete').prop('disabled', true);

 $.ajax({
 url: `/Admin/Menus/Delete/${currentMenuId}`,
 type: 'DELETE',
 headers: { 'RequestVerificationToken': token },
 success: function (response) {
 if (response.success) {
 $('#deleteModal').modal('hide');
 showAlert('success', response.message);
 loadMenus();
 } else {
 showAlert('danger', response.message || 'Failed to delete menu.');
 }
 },
 error: function () {
 showAlert('danger', 'An error occurred while deleting the menu. Please try again.');
 },
 complete: function () {
 $('#deleteSpinner').addClass('d-none');
 $('#btnConfirmDelete').prop('disabled', false);
 }
 });
 }

 // ========================================
 // Utility Functions
 // ========================================
 function resetForm() {
 $('#menuForm')[0].reset();
 $('#menuForm').validate().resetForm();
 $('.text-danger').html('');
 $('#menuParentId').val('');
 $('#menuIsActive').prop('checked', true);
 }

 function showAlert(type, message) {
 const alert = `
 <div class="alert alert-${type} alert-dismissible fade show" role="alert">
 ${message}
 <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
 </div>
 `;
 $('#alertContainer').html(alert);
 setTimeout(function () {
 $('.alert').fadeOut('slow', function () { $(this).remove(); });
 }, 5000);
 }
});
