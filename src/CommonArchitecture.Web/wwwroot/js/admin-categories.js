// Admin Categories Index Page - JavaScript Module
// Handles CRUD operations, pagination, sorting, and search functionality

$(document).ready(function () {
    // State variables
    const areaPrefix = window.adminAreaPrefix || '/Admin';
    let currentCategoryId = null;
    let isEditMode = false;
    let currentPage = 1;
    let pageSize = 10;
    let sortBy = 'Id';
    let sortOrder = 'asc';
    let searchTerm = '';
    let searchTimeout = null;
    const token = $('input[name="__RequestVerificationToken"]').val();

    // Initialize
    loadCategories();
    initializeValidation();
    attachEventHandlers();

    // ========================================
    // Validation Setup
    // ========================================
    function initializeValidation() {
        $("#categoryForm").validate({
            rules: {
                Name: { required: true, maxlength: 100 },
                Description: { required: true, maxlength: 500 }
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
                loadCategories();
            }, 300);
        });

        // Clear search
        $('#btnClearSearch').click(function () {
            $('#searchBox').val('');
            searchTerm = '';
            currentPage = 1;
            loadCategories();
        });

        // Page size change
        $('#pageSizeSelect').change(function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadCategories();
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
            loadCategories();
        });

        // Pagination clicks
        $(document).on('click', '#pagination a', function (e) {
            e.preventDefault();
            const page = parseInt($(this).data('page'));
            if (page && page !== currentPage) {
                currentPage = page;
                loadCategories();
            }
        });

        // Create category button
        $('#btnCreateCategory').click(function () {
            isEditMode = false;
            currentCategoryId = null;
            $('#categoryModalLabel').text('Create Category');
            $('#saveButtonText').text('Save Category');
            resetForm();
            $('#categoryModal').modal('show');
        });

        // Form submission
        $('#categoryForm').submit(function (e) {
            e.preventDefault();
            if (!$(this).valid()) return;
            saveCategory();
        });

        // Confirm delete
        $('#btnConfirmDelete').click(function () {
            deleteCategory();
        });

        // Reset form on modal close
        $('#categoryModal').on('hidden.bs.modal', resetForm);

        // Import form
        $('#importForm').on('submit', function(e) {
            e.preventDefault();
            
            var formData = new FormData();
            var fileInput = $('#importFile')[0];
            
            if(fileInput.files.length === 0){
                alert("Please select a file");
                return;
            }
            
            formData.append('file', fileInput.files[0]);
            
            $('#btnImport').prop('disabled', true);
            $('#importSpinner').removeClass('d-none');
            $('#importButtonText').text('Importing...');
            
            $.ajax({
                url: '/Admin/Categories/Import',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function(response) {
                    if(response.success) {
                        $('#importModal').modal('hide');
                        $('#importForm')[0].reset();
                        loadCategories(); 
                    } else {
                        alert(response.message || "Import failed");
                    }
                },
                error: function() {
                    alert("An error occurred during import.");
                },
                complete: function() {
                    $('#btnImport').prop('disabled', false);
                    $('#importSpinner').addClass('d-none');
                    $('#importButtonText').text('Import Categories');
                }
            });
        });
    }

    // ========================================
    // Sorting Functions
    // ========================================
    function updateSortIndicators() {
        $('.sortable').removeClass('active');
        $('.sortable i').removeClass('bi-arrow-up bi-arrow-down').addClass('bi-arrow-down-up');
        const activeHeader = $(`.sortable[data-column="${sortBy}"]`);
        activeHeader.addClass('active');
        activeHeader.find('i').removeClass('bi-arrow-down-up')
            .addClass(sortOrder === 'asc' ? 'bi-arrow-up' : 'bi-arrow-down');
    }

    // ========================================
    // Data Loading Functions
    // ========================================
    function loadCategories() {
        $('#loadingIndicator').show();
        $('#categoriesTable').hide();
        $('#emptyState').hide();

        const params = {
            PageNumber: currentPage,
            PageSize: pageSize,
            SortBy: sortBy,
            SortOrder: sortOrder
        };

        if (searchTerm) params.SearchTerm = searchTerm;

        $.ajax({
            url: `${areaPrefix}/Categories/GetAll`,
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingIndicator').hide();
                if (response.success) {
                    renderCategories(response.data);
                }
            },
            error: function () {
                $('#loadingIndicator').hide();
                showAlert('danger', 'Error loading categories. Please try again.');
            }
        });
    }

    function loadCategoryForEdit(categoryId) {
        isEditMode = true;
        currentCategoryId = categoryId;
        $('#categoryModalLabel').text('Edit Category');
        $('#saveButtonText').text('Update Category');

        $.ajax({
            url: `${areaPrefix}/Categories/GetById/${categoryId}`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const category = response.data;
                    $('#categoryId').val(category.id);
                    $('#categoryName').val(category.name);
                    $('#categoryDescription').val(category.description);
                    $('#categoryIsActive').prop('checked', category.isActive);
                    $('#categoryModal').modal('show');
                } else {
                    showAlert('danger', 'Failed to load category details.');
                }
            },
            error: function () {
                showAlert('danger', 'Error loading category details. Please try again.');
            }
        });
    }

    // ========================================
    // Rendering Functions
    // ========================================
    function renderCategories(result) {
        const tbody = $('#categoriesTableBody');
        tbody.empty();

        if (result.items.length === 0) {
            $('#categoriesTable').hide();
            $('#emptyState').show();
            $('#pagination').empty();
            $('#resultInfo').text('');
            return;
        }

        $('#categoriesTable').show();
        $('#emptyState').hide();

        result.items.forEach(function (category) {
            const statusBadge = category.isActive
                ? `<span class="badge bg-success bg-opacity-10 text-success rounded-pill px-3">Active</span>`
                : `<span class="badge bg-warning bg-opacity-10 text-warning rounded-pill px-3">Inactive</span>`;

            const row = `
                <tr>
                    <td class="ps-4">
                        <div class="d-flex align-items-center">
                            <div class="product-icon me-3 shadow-sm border">
                                <i class="fas fa-tags"></i>
                            </div>
                            <div>
                                <h6 class="mb-0 fw-bold text-dark">${escapeHtml(category.name)}</h6>
                                <small class="text-muted d-block text-truncate" style="max-width: 250px;">${escapeHtml(category.description)}</small>
                            </div>
                        </div>
                    </td>
                    <td><span class="text-muted fw-medium">#${category.id}</span></td>
                    <td><span class="fw-bold text-dark">${escapeHtml(category.name)}</span></td>
                    <td>${statusBadge}</td>
                    <td class="pe-4 text-end">
                        <button class="btn btn-light btn-sm rounded-pill px-3 text-primary fw-bold me-1 btn-edit" data-id="${category.id}">
                            <i class="fas fa-edit me-1"></i> Edit
                        </button>
                        <button class="btn btn-light btn-sm rounded-pill px-3 text-danger fw-bold btn-delete" data-id="${category.id}" data-name="${escapeHtml(category.name)}">
                            <i class="fas fa-trash me-1"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        // Update stats
        $('#statTotalCategories').text(result.items.length);
        $('#statActiveCategories').text(result.items.filter(c => c.isActive).length);
        $('#statInactiveCategories').text(result.items.filter(c => !c.isActive).length);

        renderPagination(result);
        updateResultInfo(result);

        // Attach edit/delete handlers
        $('.btn-edit').click(function() {
            const categoryId = $(this).data('id');
            loadCategoryForEdit(categoryId);
        });

        $('.btn-delete').click(function() {
            currentCategoryId = $(this).data('id');
            $('#deleteCategoryName').text($(this).data('name'));
            $('#deleteModal').modal('show');
        });
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
        $('#resultInfo').text(`Showing ${start}-${end} of ${result.totalCount} categories`);
    }

    // ========================================
    // CRUD Operations
    // ========================================
    function saveCategory() {
        const formData = {
            Name: $('#categoryName').val(),
            Description: $('#categoryDescription').val(),
            IsActive: $('#categoryIsActive').is(':checked')
        };

        const url = isEditMode
            ? `${areaPrefix}/Categories/Edit/${currentCategoryId}`
            : `${areaPrefix}/Categories/Create`;

        $('#saveSpinner').removeClass('d-none');
        $('#btnSaveCategory').prop('disabled', true);

        $.ajax({
            url: url,
            type: isEditMode ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 
                'RequestVerificationToken': token,
                'X-CSRF-TOKEN': token
            },
            success: function (response) {
                if (response.success) {
                    $('#categoryModal').modal('hide');
                    showAlert('success', response.message);
                    loadCategories();
                } else if (response.errors) {
                    for (const [field, messages] of Object.entries(response.errors)) {
                        $(`#error-${field}`).html(messages.join('<br>'));
                    }
                } else {
                    showAlert('danger', response.message || 'An error occurred.');
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    showAlert('danger', 'Your session has expired. Please login again.');
                    setTimeout(() => {
                        window.location.href = '/Admin/Auth/Login';
                    }, 2000);
                } else if (xhr.status === 403) {
                    showAlert('danger', 'You do not have permission to perform this action.');
                } else if (xhr.status === 404) {
                    showAlert('danger', 'Category not found on server.');
                } else if (xhr.status === 500) {
                    showAlert('danger', 'Server error: ' + (xhr.responseJSON?.message || 'Please check the API logs'));
                } else {
                    showAlert('danger', `Error (${xhr.status}): ${xhr.statusText}. Check browser console for details.`);
                }
            },
            complete: function () {
                $('#saveSpinner').addClass('d-none');
                $('#btnSaveCategory').prop('disabled', false);
            }
        });
    }

    function deleteCategory() {
        $('#deleteSpinner').removeClass('d-none');
        $('#btnConfirmDelete').prop('disabled', true);

        $.ajax({
            url: `${areaPrefix}/Categories/Delete/${currentCategoryId}`,
            type: 'DELETE',
            headers: { 
                'RequestVerificationToken': token,
                'X-CSRF-TOKEN': token
            },
            success: function (response) {
                if (response.success) {
                    $('#deleteModal').modal('hide');
                    showAlert('success', response.message);
                    loadCategories();
                } else {
                    showAlert('danger', response.message || 'Failed to delete category.');
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    showAlert('danger', 'Your session has expired. Please login again.');
                    window.location.href = '/Admin/Auth/Login';
                } else if (xhr.status === 403) {
                    showAlert('danger', 'You do not have permission to perform this action.');
                } else {
                    showAlert('danger', 'An error occurred while deleting the category. Please try again.');
                }
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
        $('#categoryForm')[0].reset();
        $('#categoryForm').validate().resetForm();
        $('.text-danger').html('');
        $('#categoryIsActive').prop('checked', true);
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

    function escapeHtml(text) {
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return text.replace(/[&<>"']/g, function (m) { return map[m]; });
    }

    // Global functions for inline onclick handlers
    window.editCategory = loadCategoryForEdit;
    window.deleteCategory = function(id, name) {
        currentCategoryId = id;
        $('#deleteCategoryName').text(name);
        $('#deleteModal').modal('show');
    };
    window.loadCategories = loadCategories;
});