$(() => {
    $("form").on('submit', function () {
        let index = 0;

        $(".contribution-row").each(function () {
            if ($(this).find('.form-check-input').prop('checked') === true) {

                $(this).find('.contributor-id').attr('name', `contribution[${index}].ContributorId`);
                $(this).find('.simcha-id').attr('name', `contribution[${index}].SimchaId`);
                $(this).find('.amount').attr('name', `contribution[${index}].Amount`);
                index++;
            }
        });
    })
});
