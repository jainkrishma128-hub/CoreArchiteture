// Admin Users Index Page - JavaScript Module
// Handles CRUD operations, pagination, sorting, search, and image preview functionality

$(document).ready(function () {
    // State variables
    const areaPrefix = window.adminAreaPrefix || '/Admin';
    let currentUserId = null;
    let isEditMode = false;
    let currentPage = 1;
    let pageSize = 10;
    let sortBy = 'Id';
    let sortOrder = 'asc';
    let searchTerm = '';
    let searchTimeout = null;
    let rolesList = [];
    const token = $('input[name="__RequestVerificationToken"]').val();
    const apiBaseUrl = 'http://localhost:5089'; // API base URL for images

    // Initialize
    loadRoles();
    loadUsers();
    initializeValidation();
    attachEventHandlers();

    // ========================================
    // Validation Setup
    // ========================================
    function initializeValidation() {
        $("#userForm").validate({
            rules: {
                Name: { required: true, maxlength: 100 },
                Email: { required: true, email: true, maxlength: 255 },
                Mobile: { required: true, digits: true, minlength: 10, maxlength: 15 },
                RoleId: { required: true, min: 1 }
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
                loadUsers();
            }, 300);
        });

        // Clear search
        $('#btnClearSearch').click(function () {
            $('#searchBox').val('');
            searchTerm = '';
            currentPage = 1;
            loadUsers();
        });

        // Page size change
        $('#pageSizeSelect').change(function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadUsers();
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
            loadUsers();
        });

        // Pagination clicks
        $(document).on('click', '#pagination a', function (e) {
            e.preventDefault();
            const page = parseInt($(this).data('page'));
            if (page && page !== currentPage) {
                currentPage = page;
                loadUsers();
            }
        });

        // Create user button
        $('#btnCreateUser').click(function () {
            isEditMode = false;
            currentUserId = null;
            $('#userModalLabel').text('Create User');
            $('#saveButtonText').text('Save User');
            resetForm();
            $('#userModal').modal('show');
        });

        // Edit user button
        $(document).on('click', '.btn-edit', function () {
            const userId = $(this).data('id');
            loadUserForEdit(userId);
        });

        // Delete user button
        $(document).on('click', '.btn-delete', function () {
            currentUserId = $(this).data('id');
            $('#deleteUserName').text($(this).data('name'));
            $('#deleteModal').modal('show');
        });

        // Form submission
        $('#userForm').submit(function (e) {
            e.preventDefault();
            if (!$(this).valid()) return;
            saveUser();
        });

        // Confirm delete
        $('#btnConfirmDelete').click(function () {
            deleteUser();
        });

        // Reset form on modal close
        $('#userModal').on('hidden.bs.modal', resetForm);
    }

    // ========================================
    // Role Loading Functions
    // ========================================
    function loadRoles() {
        $.ajax({
            url: `${areaPrefix}/Users/GetAllRoles`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    rolesList = response.data;
                    populateRoleDropdown(response.data);
                }
            },
            error: function () {
                showAlert('danger', 'Error loading roles. Please try again.');
            }
        });
    }

    function populateRoleDropdown(roles) {
        const dropdown = $('#userRoleId');
        dropdown.empty();
        dropdown.append('<option value="">Select Role</option>');
        roles.forEach(function (role) {
            dropdown.append(`<option value="${role.id}">${escapeHtml(role.roleName)}</option>`);
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
    function loadUsers() {
        $('#loadingIndicator').show();
        $('#usersTable').hide();
        $('#emptyState').hide();

        const params = {
            PageNumber: currentPage,
            PageSize: pageSize,
            SortBy: sortBy,
            SortOrder: sortOrder
        };

        if (searchTerm) params.SearchTerm = searchTerm;

        $.ajax({
            url: `${areaPrefix}/Users/GetAll`,
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingIndicator').hide();
                if (response.success) {
                    renderUsers(response.data);
                }
            },
            error: function () {
                $('#loadingIndicator').hide();
                showAlert('danger', 'Error loading users. Please try again.');
            }
        });
    }

    function loadUserForEdit(userId) {
        isEditMode = true;
        currentUserId = userId;
        $('#userModalLabel').text('Edit User');
        $('#saveButtonText').text('Update User');

        $.ajax({
            url: `${areaPrefix}/Users/GetById/${userId}`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const user = response.data;
                    $('#userId').val(user.id);
                    $('#userName').val(user.name);
                    $('#userEmail').val(user.email);
                    $('#userMobile').val(user.mobile);
                    $('#userRoleId').val(user.roleId);
                    $('#existingProfileImagePath').val(user.profileImagePath || '');

                    // Set profile image preview
                    if (user.profileImagePath) {
                        $('#profileImagePreview').attr('src', `${apiBaseUrl}/${user.profileImagePath}`);
                    } else {
                        $('#profileImagePreview').attr('src', "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='150' height='150'%3E%3Crect fill='%23ddd' width='150' height='150'/%3E%3Ctext fill='%23999' font-family='sans-serif' font-size='14' dy='10.5' font-weight='bold' x='50%25' y='50%25' text-anchor='middle'%3ENo Image%3C/text%3E%3C/svg%3E");
                    }

                    $('#userModal').modal('show');
                } else {
                    showAlert('danger', 'Failed to load user details.');
                }
            },
            error: function () {
                showAlert('danger', 'Error loading user details. Please try again.');
            }
        });
    }

    // ========================================
    // Rendering Functions
    // ========================================
    function renderUsers(result) {
        const tbody = $('#usersTableBody');
        tbody.empty();

        if (result.items.length === 0) {
            $('#usersTable').hide();
            $('#emptyState').show();
            $('#pagination').empty();
            $('#resultInfo').text('');
            return;
        }

        $('#usersTable').show();
        $('#emptyState').hide();

        result.items.forEach(function (user) {
            const profileImageSrc = user.profileImagePath 
                ? `${apiBaseUrl}/${user.profileImagePath}` 
                : "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='40' height='40'%3E%3Crect fill='%23ddd' width='40' height='40'/%3E%3Ctext fill='%23999' font-family='sans-serif' font-size='10' dy='10.5' font-weight='bold' x='50%25' y='50%25' text-anchor='middle'%3E%3F%3C/text%3E%3C/svg%3E";
            
            const row = `
                <tr>
                    <td>
                        <img src="${profileImageSrc}" alt="Profile" 
                             class="rounded-circle" style="width: 40px; height: 40px; object-fit: cover;" 
                             onerror="this.src=\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='40' height='40'%3E%3Crect fill='%23ddd' width='40' height='40'/%3E%3Ctext fill='%23999' font-family='sans-serif' font-size='10' dy='10.5' font-weight='bold' x='50%25' y='50%25' text-anchor='middle'%3E%3F%3C/text%3E%3C/svg%3E\"" />
                    </td>
                    <td>${user.id}</td>
                    <td>${escapeHtml(user.name)}</td>
                    <td>${escapeHtml(user.email)}</td>
                    <td>${escapeHtml(user.mobile)}</td>
                    <td>${escapeHtml(user.roleName || 'N/A')}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-warning btn-edit" data-id="${user.id}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-danger btn-delete" data-id="${user.id}" data-name="${escapeHtml(user.name)}">
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
        $('#resultInfo').text(`Showing ${start}-${end} of ${result.totalCount} users`);
    }

    // ========================================
    // CRUD Operations
    // ========================================
    function saveUser() {
        const formData = new FormData();
        formData.append('Name', $('#userName').val());
        formData.append('Email', $('#userEmail').val());
        formData.append('Mobile', $('#userMobile').val());
        formData.append('RoleId', $('#userRoleId').val());

        const profileImageFile = $('#profileImageInput')[0].files[0];
        if (profileImageFile) {
            formData.append('ProfileImage', profileImageFile);
        }

        // For update, include existing image path
        if (isEditMode) {
            const existingPath = $('#existingProfileImagePath').val();
            if (existingPath) {
                formData.append('ExistingProfileImagePath', existingPath);
            }
        }

        const url = isEditMode
            ? `${areaPrefix}/Users/Edit/${currentUserId}`
            : `${areaPrefix}/Users/Create`;

        $('#saveSpinner').removeClass('d-none');
        $('#btnSaveUser').prop('disabled', true);

        $.ajax({
            url: url,
            type: isEditMode ? 'PUT' : 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#userModal').modal('hide');
                    showAlert('success', response.message);
                    loadUsers();
                } else if (response.errors) {
                    for (const [field, messages] of Object.entries(response.errors)) {
                        $(`#error-${field}`).html(messages.join('<br>'));
                    }
                } else {
                    showAlert('danger', response.message || 'An error occurred.');
                }
            },
            error: function () {
                showAlert('danger', 'An error occurred while saving the user. Please try again.');
            },
            complete: function () {
                $('#saveSpinner').addClass('d-none');
                $('#btnSaveUser').prop('disabled', false);
            }
        });
    }

    function deleteUser() {
        $('#deleteSpinner').removeClass('d-none');
        $('#btnConfirmDelete').prop('disabled', true);

        $.ajax({
            url: `${areaPrefix}/Users/Delete/${currentUserId}`,
            type: 'DELETE',
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#deleteModal').modal('hide');
                    showAlert('success', response.message);
                    loadUsers();
                } else {
                    showAlert('danger', response.message || 'Failed to delete user.');
                }
            },
            error: function () {
                showAlert('danger', 'An error occurred while deleting the user. Please try again.');
            },
            complete: function () {
                $('#deleteSpinner').addClass('d-none');
                $('#btnConfirmDelete').prop('disabled', false);
            }
        });
    }

    // ========================================
    // Image Preview Functions
    // ========================================
    window.previewProfileImage = function(input) {
        if (input.files && input.files[0]) {
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#profileImagePreview').attr('src', e.target.result);
            };
            reader.readAsDataURL(input.files[0]);
        }
    };

    // ========================================
    // Utility Functions
    // ========================================
    function resetForm() {
        $('#userForm')[0].reset();
        $('#userForm').validate().resetForm();
        $('.text-danger').html('');
        $('#profileImagePreview').attr('src', "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='150' height='150'%3E%3Crect fill='%23ddd' width='150' height='150'/%3E%3Ctext fill='%23999' font-family='sans-serif' font-size='14' dy='10.5' font-weight='bold' x='50%25' y='50%25' text-anchor='middle'%3ENo Image%3C/text%3E%3C/svg%3E");
        $('#existingProfileImagePath').val('');
        $('#profileImageInput').val('');
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
        if (!text) return '';
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return text.replace(/[&<>"']/g, function (m) { return map[m]; });
    }
});

