$(() => {
    $("#new-contributor").on('click', function () {
        new bootstrap.Modal($('.new-contrib')[0]).show();
    })

    $(".deposit-button").on('click', function () {
        $("#deposit-name").text($(this).data("name"))
        $("#id").val($(this).data("contributor-id"))
        new bootstrap.Modal($('.deposit')[0]).show();
    })

    $(".edit-contrib").on('click', function () {
        $(".modal-title").text("Edit Contributor")
        $("#contributor_first_name").val($(this).data("first-name"))
        $("#contributor_last_name").val($(this).data("last-name"))
        $("#contributor_cell_number").val($(this).data("cell"))
        $("#contributor_always_include").prop('checked', $(this).data("always-include") === 'True')
        $("#contributor_id").val($(this).data("id"))
        $("#initialDepositDiv").remove()
        $("#createddate").remove();

        new bootstrap.Modal($('.new-contrib')[0]).show();
    });
})
