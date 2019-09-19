
$('#agrModelForm').on('click', function () {
    //AggregatorSaveChanges
    var x = $('#AgrOutputFlags').val();
    var model = {
        //ConnectionName: $('#connectionList').val(),
        SourceOutputFlags: $('#AgrOutputFlags').val(),
        //TableName: $('#sourceTableList').val(),
        SourceName: activeContainerId

    }

    $.ajax({
        url: "Home/AggregatorSaveChanges",
        type: "Post",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(model), //if you need to post Model data, use this
        success: function (result) {
            $("#partial").html(result);
        }
    });
});

function createCheckBox(columnId, isChecked, fieldLabel) {
    var col = '<td >' +
        '<div class="custom-control custom-checkbox"> ' +
        '<input type="checkbox" class="checkbox" ' + isChecked + ' id = "' +
        columnId + fieldLabel +
        '"  data-id = "' +
        columnId + fieldLabel +
        '" >' + '</div>' + '</td>';
    return col;
}

function aggregatorSettings(activeContainerId) {
    $.getJSON('/home/AggregatorSettings', { containerId: activeContainerId }, function (data) {
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
        var sourceModel = jsonData.SourceModel;
        var inputModel = jsonData.InputModel;
        $('#aggregator-table-name').empty();
        $('#aggregator-table-name').text(sourceModel.ConnectionName + ' -> ' + sourceModel.TableName);

        $('#aggregator-table-body').empty();
        var checkFalse = ' value = "false"';
        var checkTrue = ' value = "true" checked="checked" ';
        var trHTML = '';
        var tdHTML = '';
        var names = [];
        for (var i = 0; i < inputModel.length; i++) //The json object has lenght
        {
            tdHTML = '';
            console.log(inputModel[i]);
            //isChecked = ' value = "false" disabled="disabled"';
            var isCheck = [checkFalse, checkFalse, checkFalse, checkFalse, checkFalse, checkFalse];
            if (inputModel[i].GroupByFlag === true) {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkTrue, '-Group');
            }
            else {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkFalse, '-Group');
            }
            if (inputModel[i].CountFlag === true) {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkTrue, '-Count');
            }
            else {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkFalse, '-Count');
            }
            if (inputModel[i].SumFlag === true) {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkTrue, '-Sum');
            }
            else {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkFalse, '-Sum');
            }
            if (inputModel[i].MaxFlag === true) {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkTrue, '-Max');
            }
            else {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkFalse, '-Max');
            }
            if (inputModel[i].MinFlag === true) {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkTrue, '-Min');
            }
            else {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkFalse, '-Min');
            }
            if (inputModel[i].AvgFlag === true) {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkTrue, '-Avg');
            }
            else {
                tdHTML += createCheckBox(inputModel[i].ColumnId, checkFalse, '-Avg');
            }
            trHTML += '<tr>' +
                '<td >' +
                inputModel[i].ColumnName +
                '</td>' +
                tdHTML +
                ' </tr>';
        }
        
        $('#aggregator-table-body').append(trHTML);


    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting aggregation settings!');
    });
}

//---------------Aggregator Checkbox Handling--------------
$('#aggregator-table').on('change',
    'input[type="checkbox"]',
    function (e) {
        var chkVal = $(this).prop('value');
        //find("[data-id= 0]").prop('value');
        if (chkVal === 'true') {
            $(this).prop('value', 'false').removeAttr('checked');
        } else {
            $(this).prop('value', 'true').prop('checked', 'checked');
            //$('#sourceTable').find("[data-id= 0]").prop('value', 'true').prop('checked', 'checked');
        }
        updateAggregatorOutputFlag();
    });

function updateAggregatorOutputFlag() {
    var checkAggregatorIds = [];
    $('#aggregator-table input[type="checkbox"]').each(function (idx, val) {
        if ($('#myCheckbox').is(':disabled')) {

        }
        else if ($(this).prop('checked')) {
            checkAggregatorIds.push($(this).data('id'));
        }
    });
    $('#AgrOutputFlags').val(checkAggregatorIds);

}

//---------------Aggregator Checkbox Handling--------------
$('#create_aggregator').click(function () {
    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            if (jsonData.Project == -1 && jsonData.Mapping == -1) {
                alert('Please select a Project/ Mapping');
            }
            else {
                var operatorId = 'aggregator_' + aggregator_operatorI;
                var operatorData = {
                    top: 60,
                    left: 500,
                    opId: operatorId,
                    properties: {
                        title: 'aggregator',
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
                aggregator_operatorI++;

                $('#example').flowchart('createOperator', operatorId, operatorData);
                var source = "aggregator";
                //alert(source + ' - ' + operatorId);
                updateContainerVal(source, operatorId);
                Logs(operatorId + " is created <br />");
            }
        });
});

/*
 outCol = '<td >' +
    inputModel[i].ColumnName +
    '</td>' +
    '<td > <div class="custom-control custom-checkbox"> ' +
    '<input type="checkbox" class="checkbox" ' + isChecked + ' id = "' +
    inputModel[i].ColumnId +
    '"  data-id = "' +
    inputModel[i].ColumnId +
    '" >';
    names.push(outCol);
    outCol = ''; 
    */
/*for (var i = 0, j = 0; i < sourceModel.InputModel.length; i++ , j++) //The json object has lenght
        {
            isChecked = ' value = "false"';
            if (sourceModel.InputModel[i].OutputFlag === true) {
                isChecked = ' value = "true" checked="checked" ';
            }
            if (!j < names.length) {
                outCol = '';
            }
            trHTML += '<tr>' +
                '<td>' +
                sourceModel.InputModel[i].ColumnName +
                '</td>' +
                '<td> <div class="custom-control custom-checkbox"> ' +
                '<input type="checkbox" class="checkbox " disabled ' + isChecked + ' >' +
                '</div> </td> ' + names[j] + ' </tr>';
        }*/