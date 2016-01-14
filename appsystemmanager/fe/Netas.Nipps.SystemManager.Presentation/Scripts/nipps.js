var AjaxLoadingHtml = '<div style="margin: auto; padding: 0px 0px 0px 0px; width: 32px;"><img src="../Images/ajax-loading.gif"/></div>';


$.fn.extend(
{

    Nipps_FillOption: function (postTo, postData) {
        
        var selectObject = $(this);
        selectObject.empty();

        $.ajax({
            type: 'POST',
            dataType: 'json',
            url: postTo,
            data: postData,

            success: function (options) {
                $.each(options, function (i, option) {
                    selectObject.append('<option value="' + option.Value + '">' + option.Text + '</option>');
                });
            },

            error: function (ex) {
                console.log(ex);
            }
        });

        return this;
    },

    Nipps_OpenModalDialog: function (paramTitle, paramUrl, paramWidth, paramHeight) {

        $(this).dialog({

            width: paramWidth,
            height: paramHeight,
            title: paramTitle,
            resizable: false,
            modal: true,

            open: function (event, ui) {
                $(this).html(AjaxLoadingHtml);
                $(this).load(encodeURI(paramUrl));
            },

            buttons: {
                "Close": function () {
                    $(this).dialog("close");
                    $(this).html('');
                }
            }

        });

        return false;

    },

    Nipps_SubmitTo: function (url, method) {
        var _this = $(this);
        _this.attr('action', encodeURI(url));
        _this.attr('method', method);
        _this.submit();
    },

    Nipps_AjaxDialog: function(title, height, width, url, closeButtonText, buttons) {

        var _this = $(this);

        _this.dialog( {
            
            title: title,
            height: height,
            width: width,
            
            open: function (event, ui) {
                $(this).html(AjaxLoadingHtml);
                $(this).load(encodeURI(url));
            }
        } );

        var _buttons = new Array();

        $.each(buttons, function(i, button) {
            
            _buttons.push( {
                text: button.text,
                click: function() {
                    console.log(button.url, button.dataType);
                    $.ajax({
                        url: button.url,
                        data: button.postData,
                        dataType: button.dataType,
                        success: function(data) {
                            console.log(data);
                            $(button.target).html(data);
                        },
                        error: function(xhr, status, error) {
                            $(button.target).html(error);
                        }
                    });
                }
            } );

        } );	

        _buttons.push({
            text: closeButtonText,
            click: function() {
                $(this).dialog("close");
            }
        });

        _this.dialog("option", "buttons", _buttons);

        return false;
    }

});

