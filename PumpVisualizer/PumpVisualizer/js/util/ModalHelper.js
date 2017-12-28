function createModal(heading, formContent, strSubmitFunc, btnSubmitText, btnCloseText)
    {
        var html =  '<div id="modalWindow" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="confirm-modal" aria-hidden="true">';
        html += '<div class="modal-dialog">';
        html += '<div class="modal-content">';
        html += '<div class="modal-header">';
        html += '<a class="close" data-dismiss="modal">×</a>';
        html += '<h4 class="text-primary">' + heading + '</h4>';
        html += '</div>';
        html += '<div class="modal-body">';
        html += formContent;
        html += '</div>';
        html += '<div class="modal-footer">';
        if (btnSubmitText!='') {
            html += '<span class="btn btn-success"';
            html += ' onClick="'+strSubmitFunc+'">'+btnSubmitText;
            html += '</span>';
        }
        html += '<span class="btn btn-default" data-dismiss="modal">';
        html += btnCloseText;
        html += '</span>'; // close button
        html += '</div>';  // footer
        html += '</div>';  // content
        html += '</div>';  // dialog
        html += '</div>';  // modalWindow
        $('body').append(html);
        //$("#modalWindow").modal();
        $("#modalWindow").modal('show');

        $('#modalWindow').on('hide.bs.modal', function (e) {
            
            $(this).remove();
            
        });
}