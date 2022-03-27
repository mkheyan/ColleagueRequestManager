window.ShowToastr = (type, message) => {
    if (type === "success") {
        toastr.success(message, "Operation Successful")
    }
    if (type === "error") {
        toastr.error(message, "Operation Failed")
    }
}

window.ShowSwal = (type, message) => {
    if (type === "success") {
        Swal.fire({
            title: 'Success',
            text: message,
            icon: 'success',
            confirmButtonText: 'OK'
        })
    }
    if (type === "error") {
        Swal.fire({
            title: 'Error',
            text: message,
            icon: 'error',
            confirmButtonText: 'Okay'
        })
    }
}

function ShowDeleteConfirmationModal() {
    $('#deleteConfirmationModal').modal('show');
}

function HideDeleteConfirmationModal() {
    $('#deleteConfirmationModal').modal('hide');
}