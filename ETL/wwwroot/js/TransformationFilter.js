
$('#create_filter').click(function () {
    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            if (jsonData.Project == -1 && jsonData.Mapping == -1) {
                alert('Please select a Project/ Mapping');
            }
            else {
                var operatorId = 'filter_' + filter_operatorI;
                var operatorData = {
                    top: 60,
                    left: 500,
                    opId: operatorId,
                    properties: {
                        title: 'filter',
                        inputs: {
                            input_1: {
                                label: 'Input 1',
                            }
                        },
                        outputs: {
                            output_1: {
                                label: 'Output 1',
                            }
                        }
                    }
                };

                filter_operatorI++;

                $('#example').flowchart('createOperator', operatorId, operatorData);
                var source = "filter";
                //alert(source + ' - ' + operatorId);
                updateContainerVal(source, operatorId);
                Logs(operatorId + " is created <br />");
            }
        });
});

$(document).ready(function () {
});

function setFilterModelRowDDOptions(index) {
    $('#filter-select-condition-' + index).empty();
    var options = "<option value=''>--Select Condition--</option>" +
        "<option value='AND'>AND</option>" +
        "<option value='OR'>OR</option>";
    $('#filter-select-condition-' + index).append(options);
    $('#filter-select-operator-' + index).empty();
    var options = "<option value=''>--Select Operator--</option>" +
        "<option value='Equal'>Equal</option>" +
        "<option value='IN'>In</option>" +
        "<option value='NOTIN'>Not In</option>" +
        "<option value='LIKE'>Like</option>";
    $('#filter-select-operator-' + index).append(options);
    if (index > 0) {
        $('#filter-select-column-' + index).empty();
        var $options = $("#filter-select-column-0 > option").clone();
        $('#filter-select-column-' + index).append($options);
    }
}

function filterSettings(activeContainerId) {
    $.getJSON('/home/FilterSettings', { containerId: activeContainerId }, function (data) {
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
        var inputModel = jsonData.InputModel;
        var selectedFilterModel = jsonData.SelectedFilterModel;
        var index = 0;
        
        console.log('Before ' + $('#filter-table-body tbody tr').length + ':' + selectedFilterModel.length);
        if (selectedFilterModel.length > 0) {

            $('#filter-table-body').empty();
            filter_row_count = selectedFilterModel.length - 1;
            for (var i = 0; i < selectedFilterModel.length; i++) {
                
                //if (i >= 2) {
                    var html = addRowHTML(i);
                    $('#filter-table-body').append(html);
                //}
                if (i == 0) {
                    var options = "<option>--Select Column--</option>";
                    for (var j = 0; j < inputModel.length; j++) {
                        options += "<option vlaue='" + inputModel[j].ColumnName + "'>" + inputModel[j].ColumnName + "</option>";
                    }
                    $('#filter-select-column-0').append(options);
                }
                setFilterModelRowDDOptions(i);
                var model = selectedFilterModel[i];
                $('#filter-select-column-' + i).val(model.ColumnName);
                $('#filter-select-operator-' + i).val(model.FilterOperator);
                $('#filter-select-condition-' + i).val(model.FilterCondition);
                if (model.SelfCheck === 'true') { //drop down
                    $('#filter-column-value-dd-' + i).empty();
                    var options = "<option>--Select Column--</option>";
                    for (var j = 0; j < inputModel.length; j++) {
                        options += "<option vlaue='" + inputModel[j].ColumnName + "'>" + inputModel[j].ColumnName + "</option>";
                    }
                    $('#filter-column-value-dd-' + i).append(options);
                    $('#filter-column-value-dd-' + i).val(model.FilterValue);
                } else {
                    $('#filter-column-value-text-' + i).val(model.FilterValue);
                }
            }
        }
        else {
            $('#filter-table-body').empty();
            var defaultPos = 0;
            var html = addRowHTML(defaultPos);
            $('#filter-table-body').append(html);
            $('#filter-column-value-dd-' + defaultPos).empty();
            var options = "<option>--Select Column--</option>";
            for (var j = defaultPos; j < inputModel.length; j++) {
                options += "<option vlaue='" + inputModel[j].ColumnName + "'>" + inputModel[j].ColumnName + "</option>";
            }
            $('#filter-select-column-' + defaultPos).append(options);
            setFilterModelRowDDOptions(defaultPos);//set FilterModel Drop down options
            //setFilterModelRowDDOptions(1);//set FilterModel Drop down options
        }
        //FilterModel -- set filterModel first row options
        
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting Filter settings!');
    });
}
function addRowHTML(counterId) {
    var html = "<tr > <td> <select id ='filter-select-column-" + counterId + "' class='form-control'></select > </td > " +
        "<td class='text-center filter-self-column'>" +
        "<input id='filter-self-column-" + counterId + "' type='checkbox' class='' data-recId='" + counterId + "' />" +
        "</td>" +
        "<td>" +
        "<select id='filter-select-operator-" + counterId + "' class='form-control'></select>" +
        "</td>" +
        "<td class='filter-column-value-option-text-" + counterId + " id='filter-column-value-option-text-0'>" +
        "<input id='filter-column-value-text-" + counterId + "' type='text' class='form-control' />" +
        "</td>" +
        "<td class='filter-column-value-option-dd-" + counterId + " hide' id='filter-column-value-option-dd-" + counterId + ">" +
        "<select id='filter-column-value-dd-" + counterId + " class='form-control'></select>'" +
        "</td>" +
        "<td>" +
        "<select id='filter-select-condition-" + counterId + "' class='form-control'></select>" +
        "</td>" +
        "<td>" + "<input id='filter-delete-row-" + counterId + "' type='button' class='btn btn-warning btn-sm pull-right filter-delete-row' value='Remove' />" + "</td>" +
        //"<td>" +
        //"<select id='filter-select-order-" + counterId + "' class='form-control' disabled></select>" +
        //"</td> " +
        "</tr>";
    return html;
}

$('tbody').on('click', '.filter-delete-row', function () {
    // do something
    alert($(this).prop('id'));
    $(this).parent("td:first").parent("tr:first").remove();
});

$('.filter-delete-row input[type = "button"]').on('click', function (e) {
    alert();
})

//    input[type = "button"]').on('click', function (e) {
   
$('#filter-add-row').on('click', function (e) {
    filter_row_count++;
    var counterid = filter_row_count;
    var html = addRowHTML(counterid);
    $('#filter-table-body').append(html);
    setFilterModelRowDDOptions(counterid);
});

$('.filter-self-column input[type="checkbox"]').on('change', function () {
    var log = "";

    var chkVal = $(this).prop('value');
    log = log + " -- " + chkVal;
    log = log + " -- " + $(this).attr('data-recId');
    var recId = $(this).attr('data-recId');
    var cbId = $(this).attr('id');
    if (chkVal === 'true') {
        $(this).prop('value', 'false').removeAttr('checked');
        $('.filter-column-value-option-dd-' + recId).addClass('hide');
        $('.filter-column-value-option-text-' + recId).removeClass('hide');

        $('#filter-column-value-text' + recId).val('Val');
    } else {
        $(this).prop('value', 'true').prop('checked', 'checked');
        $('.filter-column-value-option-text-' + recId).addClass('hide');
        $('.filter-column-value-option-dd-' + recId).removeClass('hide');

        var $options = $("#filter-select-column-0 > option").clone();
        log += $options;
        $('#filter-column-value-dd-' + recId).empty();
        $('#filter-column-value-dd-' + recId).append($options);

        //$('#sourceTable').find("[data-id= 0]").prop('value', 'true').prop('checked', 'checked');
    }

});

$('#filterModelForm').on('click', function () {
    var filterRowCount = filter_row_count;
    var res = "";
    var rowObj = [];
    $('#filter-table tbody tr').each(function (i) {
        
        var model = {
            ColumnId : '-1',
            ColumnName : '',
            SelfCheck : 0,
            FilterOperator : '',
            FilterValue : '-1',
            FilterCondition : ''
        }
        var isSelected = 'false';
        //alert($(this).children('td').length + '' + i);
        //alert('Text:' + $(this).children('input[type="text"]').length + 'DD:' + $(this).children('select'));

        $(this).children('td').each(function (j) {
            model.ColumnId = i;
            
            $(this).children('input[type="text"]').each(function () {
                model.FilterValue = $(this).val().trim();
            });
            $(this).children('input[type="checkbox"]').each(function () {
                model.SelfCheck = $(this).val();
                if ($(this).val() === 'true') {
                    isSelected = 'true';
                }
            });
            $(this).children('select').each(function () {
                if (j == 0) {
                    model.ColumnName = $(this).val();
                } else if (j == 2) {
                    model.FilterOperator = $(this).val();
                } else if (j == 4) {
                    if (isSelected === 'true') {
                        model.FilterValue = $(this).val();
                        console.log('Selected');
                    }
                } else if (j == 5) {
                    model.FilterCondition = $(this).val();
                }
            });
        });
        console.log(model);
        rowObj.push(model);
    })
    var model = {
        FilterName: activeContainerId,
        SelectedFilterModel: rowObj
    };
    $.ajax({
        url: "Home/FilterSaveChanges",
        type: "Post",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(model), //if you need to post Model data, use this
        success: function (result) {
            $("#partial").html(result);
        }
    });
});

//---------------Expression--------------

$('#create_expression').click(function () {
    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            if (jsonData.Project == -1 && jsonData.Mapping == -1) {
                alert('Please select a Project/ Mapping');
            }
            else {
                var operatorId = 'expression_' + expression_operatorI;
                var operatorData = {
                    top: 60,
                    left: 500,
                    opId: operatorId,
                    properties: {
                        title: 'expression',
                        inputs: {
                            input_1: {
                                label: 'Input 1',
                            }
                        },
                        outputs: {
                            output_1: {
                                label: 'Output 1',
                            }
                        }
                    }
                };

                expression_operatorI++;

                $('#example').flowchart('createOperator', operatorId, operatorData);
                var source = "expression";
                //alert(source + ' - ' + operatorId);
                updateContainerVal(source, operatorId);
                Logs(operatorId + " is created <br />");
            }
        });
});

function expressionSettings(activeContainerId) {
    $.getJSON('/home/ExpressionSettings', { containerId: activeContainerId }, function (data) {
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
        var sourceModel = jsonData.SourceModel;
        var inputModel = jsonData.InputModel;
        $('#expression-table-name').empty();
        $('#expression-table-name').text(sourceModel.ConnectionName + ' -> ' + sourceModel.TableName);

        $('#expression-table-body').empty();
        var trHTML = '';
        var names = [];
        for (var i = 0; i < inputModel.length; i++) //The json object has lenght
        {

            if (inputModel[i].OutputFlag === true) {

                trHTML += '<tr>' + '<td >' +
                    inputModel[i].ColumnName +
                    '</td>' +
                    '<td > <div class="col-md-6"> ' +
                    '<select  class="ASD" id = "' +
                    inputModel[i].ColumnId + 'exp' +
                    '"  data-id = "' +
                    inputModel[i].ColumnId +
                    '" ></select>'
                ' </tr>';

            }

        }
        $('#expression-table-body').append(trHTML);

        for (var i = 0; i < inputModel.length; i++) //The json object has lenght
        {
            if (inputModel[i].OutputFlag === true) {
                console.log($('#' + inputModel[i].ColumnId));
                //  $('#' + inputModel[i].ColumnId).empty();
                $('#' + inputModel[i].ColumnId + 'exp').append('<option >' + '--Select DataType--' + '</option>');
                $('#' + inputModel[i].ColumnId + 'exp').append('<option >' + 'Integar' + '</option>');
                $('#' + inputModel[i].ColumnId + 'exp').append('<option >' + 'Float' + '</option>');
                $('#' + inputModel[i].ColumnId + 'exp').append('<option >' + 'Double' + '</option>');
                $('#' + inputModel[i].ColumnId + 'exp').append('<option >' + 'Date' + '</option>');

                if (inputModel[i].toDataType != null && inputModel[i].toDataType != '')
                    $('#' + inputModel[i].ColumnId + 'exp').val(inputModel[i].toDataType);
            }
        }

    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting expression settings!');
    });
}

$('#expModelForm').on('click', function () {
    //AggregatorSaveChanges
    var model = new Array();
    $("#expression-table TBODY TR").each(function () {
        var row = $(this);
        var obj = {
            ExpressionName: activeContainerId,
            columnName: row.find("TD").eq(0).html(),
            DataType: row.find("select").val()
            //   var $td3 = $td.find('select');
        }
        model.push(obj);
    });
    console.log(model);

    $.ajax({
        url: "Home/ExpressionSaveChanges",
        type: "Post",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(model), //if you need to post Model data, use this
        success: function (result) {
            $("#partial").html(result);
        }
    });
});
