// Roles Index Page - JavaScript Module
// Handles CRUD operations, pagination, sorting, and search functionality

$(document).ready(function () {
    // State variables
    let currentRoleId = null;
    let isEditMode = false;
    let currentPage = 1;
    let pageSize = 10;
    let sortBy = 'Id';
    let sortOrder = 'asc';
    let searchTerm = '';
    let searchTimeout = null;
    const token = $('input[name="__RequestVerificationToken"]').val();

    // Initialize
    loadRoles();
    initializeValidation();
    attachEventHandlers();

    // ========================================
    // Validation Setup
    // ========================================
    function initializeValidation() {
        $("#roleForm").validate({
            rules: {
                RoleName: { required: true, maxlength: 100 }
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
                loadRoles();
            }, 300);
        });

        // Clear search
        $('#btnClearSearch').click(function () {
            $('#searchBox').val('');
            searchTerm = '';
            currentPage = 1;
            loadRoles();
        });

        // Page size change
        $('#pageSizeSelect').change(function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadRoles();
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
            loadRoles();
        });

        // Pagination clicks
        $(document).on('click', '#pagination a', function (e) {
            e.preventDefault();
            const page = parseInt($(this).data('page'));
            if (page && page !== currentPage) {
                currentPage = page;
                loadRoles();
            }
        });

        // Create role button
        $('#btnCreateRole').click(function () {
            isEditMode = false;
            currentRoleId = null;
            $('#roleModalLabel').text('Create Role');
            $('#saveButtonText').text('Save Role');
            resetForm();
            $('#roleModal').modal('show');
        });

        // Edit role button
        $(document).on('click', '.btn-edit', function () {
            const roleId = $(this).data('id');
            loadRoleForEdit(roleId);
        });

        // Delete role button
        $(document).on('click', '.btn-delete', function () {
            currentRoleId = $(this).data('id');
            $('#deleteRoleName').text($(this).data('name'));
            $('#deleteModal').modal('show');
        });

        // Form submission
        $('#roleForm').submit(function (e) {
            e.preventDefault();
            if (!$(this).valid()) return;
            saveRole();
        });

        // Confirm delete
        $('#btnConfirmDelete').click(function () {
            deleteRole();
        });

        // Reset form on modal close
        $('#roleModal').on('hidden.bs.modal', resetForm);
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
    function loadRoles() {
        $('#loadingIndicator').show();
        $('#rolesTable').hide();
        $('#emptyState').hide();

        const params = {
            PageNumber: currentPage,
            PageSize: pageSize,
            SortBy: sortBy,
            SortOrder: sortOrder
        };

        if (searchTerm) params.SearchTerm = searchTerm;

        $.ajax({
            url: '/Roles/GetAll',
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingIndicator').hide();
                if (response.success) {
                    renderRoles(response.data);
                }
            },
            error: function () {
                $('#loadingIndicator').hide();
                showAlert('danger', 'Error loading roles. Please try again.');
            }
        });
    }

    function loadRoleForEdit(roleId) {
        isEditMode = true;
        currentRoleId = roleId;
        $('#roleModalLabel').text('Edit Role');
        $('#saveButtonText').text('Update Role');

        $.ajax({
            url: `/Roles/GetById/${roleId}`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const role = response.data;
                    $('#roleId').val(role.id);
                    $('#roleName').val(role.roleName);
                    $('#roleModal').modal('show');
                } else {
                    showAlert('danger', 'Failed to load role details.');
                }
            },
            error: function () {
                showAlert('danger', 'Error loading role details. Please try again.');
            }
        });
    }

    // ========================================
    // Rendering Functions
    // ========================================
    function renderRoles(result) {
        const tbody = $('#rolesTableBody');
        tbody.empty();

        if (result.items.length === 0) {
            $('#rolesTable').hide();
            $('#emptyState').show();
            $('#pagination').empty();
            $('#resultInfo').text('');
            return;
        }

        $('#rolesTable').show();
        $('#emptyState').hide();

        result.items.forEach(function (role) {
            const row = `
                <tr>
                    <td>${role.id}</td>
                    <td>${escapeHtml(role.roleName)}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-warning btn-edit" data-id="${role.id}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-danger btn-delete" data-id="${role.id}" data-name="${escapeHtml(role.roleName)}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

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
        $('#resultInfo').text(`Showing ${start}-${end} of ${result.totalCount} roles`);
    }

    // ========================================
    // CRUD Operations
    // ========================================
    function saveRole() {
        const formData = {
            RoleName: $('#roleName').val()
        };

        const url = isEditMode
            ? `/Roles/Edit/${currentRoleId}`
            : '/Roles/Create';

        $('#saveSpinner').removeClass('d-none');
        $('#btnSaveRole').prop('disabled', true);

        $.ajax({
            url: url,
            type: isEditMode ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#roleModal').modal('hide');
                    showAlert('success', response.message);
                    loadRoles();
                } else if (response.errors) {
                    // Display server-side validation errors
                    for (const [field, messages] of Object.entries(response.errors)) {
                        $(`#error-${field}`).html(messages.join('<br>'));
                    }
                } else {
                    showAlert('danger', response.message || 'An error occurred.');
                }
            },
            error: function () {
                showAlert('danger', 'An error occurred while saving the role. Please try again.');
            },
            complete: function () {
                $('#saveSpinner').addClass('d-none');
                $('#btnSaveRole').prop('disabled', false);
            }
        });
    }

    function deleteRole() {
        $('#deleteSpinner').removeClass('d-none');
        $('#btnConfirmDelete').prop('disabled', true);

        $.ajax({
            url: `/Roles/Delete/${currentRoleId}`,
            type: 'DELETE',
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#deleteModal').modal('hide');
                    showAlert('success', response.message);
                    loadRoles();
                } else {
                    showAlert('danger', response.message || 'Failed to delete role.');
                }
            },
            error: function () {
                showAlert('danger', 'An error occurred while deleting the role. Please try again.');
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
        $('#roleForm')[0].reset();
        $('#roleForm').validate().resetForm();
        $('.text-danger').html('');
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
});

