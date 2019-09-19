$(function () {
    //----------New Project----------
    $('#new_project_create').on('click', function () {
        $('.spinner').css('display', 'block');
        var projectName = $('#new_project_name').val();
        var model = {
            name: projectName
        }
        $.ajax({
            url: "Home/CreateProject",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(model), //if you need to post Model data, use this
            success: function (data) {
                $('#html1').jstree(true).settings.core.data = data.nodes;
                $('#html1').jstree(true).refresh();
                $('#new_project_name').val('');
                Logs(data.message + "<br />");
                $('.spinner').css('display', 'none');
            },
            failure: function (result) {
                $('.spinner').css('display', 'none');
                alert('Failed');
            }
        });
    });


    $('#new_mapping_create').on('click', function () {
        $('.spinner').css('display', 'block');
        var selectedNode = $('#html1').jstree(true).get_selected(true)
        var mappingName = $('#new_mapping_name').val();
        if (selectedNode.length == 0) {
            alert('Please select a Project/ Mapping');
            $('.spinner').css('display', 'none');
        }
        else if (selectedNode[0].id.includes("_c")) {
            alert('Please select a Project');
            $('.spinner').css('display', 'none');
        }
        else {
            var model = {
                ProjectId: selectedNode[0].id,
                MappingName: mappingName
            }
            $.ajax({
                url: "Home/CreateMapping",
                type: "Post",
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(model), //if you need to post Model data, use this
                success: function (data) {
                    $('#html1').jstree(true).settings.core.data.nodes = data;
                    $('#html1').jstree(true).refresh();
                    $('#new_mapping_name').val('');
                    Logs(data.message + "<br />");
                    $('.spinner').css('display', 'none');
                },
                failure: function (result) {
                    $('.spinner').css('display', 'none');
                    alert('Failed');
                }
            });
        }

    });
    //----------New Project----------

    $('#html1').jstree({
        'core': {
            'data': {
                'url': '/Home/Nodes',
                'data': function (node) {
                    return { 'id': node.id };
                }
            }
        }
    });


    $('#html1').on('changed.jstree', function (e, data) {
        var i, j, r = [], ii, p, t, s;
        for (i = 0, j = data.selected.length; i < j; i++) {
            ii = data.instance.get_node(data.selected[i]).id;
            p = data.instance.get_node(data.selected[i]).parent;
            t = data.instance.get_node(data.selected[i]).text;
            s = data.instance.get_node(data.selected[i]).selected;
        }

        if(p != '#' && p!='')
        SerialFlow(data);
        // $('#event_result').html('Selected: ' + r.join(', '));
    }).jstree();
});

function myPromiseDelSrc(x, sec) {
    return new Promise(resolve => {
        $('.spinner').css('display', 'block');
        var sourceElements = document.querySelectorAll('[id^="source_"]');
        if (sourceElements != null) {
            for (var i = 0, len = sourceElements.length; i < len; i++) {

                $('#example').flowchart('deleteOperatorAtStart', sourceElements[i].id);
            }
        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseDelTrg(x, sec) {
    return new Promise(resolve => {
        var targetElements = document.querySelectorAll('[id^="target_"]');
        if (targetElements != null) {
            for (var i = 0, len = targetElements.length; i < len; i++) {

                $('#example').flowchart('deleteOperatorAtStart', targetElements[i].id);
            }
        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseDelAgr(x, sec) {
    return new Promise(resolve => {
        var aggregatorElements = document.querySelectorAll('[id^="aggregator_"]');
        if (aggregatorElements != null) {
            for (var i = 0, len = aggregatorElements.length; i < len; i++) {

                $('#example').flowchart('deleteOperatorAtStart', aggregatorElements[i].id);
            }

        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseDelJoiner(x, sec) {
    return new Promise(resolve => {
        var joinerElements = document.querySelectorAll('[id^="joiner_"]');
        if (joinerElements != null) {
            for (var i = 0, len = joinerElements.length; i < len; i++) {

                $('#example').flowchart('deleteOperatorAtStart', joinerElements[i].id);
            }

        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseDelFilter(x, sec) {
    return new Promise(resolve => {
        var filterElements = document.querySelectorAll('[id^="filter_"]');
        if (filterElements != null) {
            for (var i = 0, len = filterElements.length; i < len; i++) {

                $('#example').flowchart('deleteOperatorAtStart', filterElements[i].id);
            }

        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseDelTrans(x, sec) {
    return new Promise(resolve => {
        var expressionElements = document.querySelectorAll('[id^="expression_"]');
        if (expressionElements != null) {
            for (var i = 0, len = expressionElements.length; i < len; i++) {

                $('#example').flowchart('deleteOperator', expressionElements[i].id);
            }
        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseDelLinks(x, sec) {
    return new Promise(resolve => {
        var links = document.getElementsByClassName("flowchart-link");
        if (links != null) {
            for (var i = 0, len = links.length; i < len; i++) {
                $('#example').flowchart('deleteLinkAtStart', links[i].link_id);

            }
        }
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

function myPromiseload(x, sec, data) {
    return new Promise(resolve => {
        var i, j, r = [], ii, p, t, s;
        for (i = 0, j = data.selected.length; i < j; i++) {
            ii = data.instance.get_node(data.selected[i]).id;
            p = data.instance.get_node(data.selected[i]).parent;
            t = data.instance.get_node(data.selected[i]).text;
            s = data.instance.get_node(data.selected[i]).selected;
        }


        var model = {
            id: ii,
            parent: p,
            text: t,
            selected: s

        }
        $.ajax({
            url: "Home/Load",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(model), //if you need to post Model data, use this
            success: function (data) {
                var jsonData = jQuery.parseJSON(JSON.stringify(data));
                //console.log(jsonData);
                //alert(jsonData.Id);
                if (jsonData != null) {
                    //console.log(jsonData.SourceDictionary);
                    //$('#example').load('#example');
                    var source_op = null;
                    var sop;
                    for (var s in jsonData.SourceDictionary) {
                        if (jsonData.SourceDictionary.hasOwnProperty(s)) {
                            //console.log(s + '   ' + jsonData.SourceDictionary[s].SourceName);
                            var operatorId = jsonData.SourceDictionary[s].SourceName;
                            source_op = operatorId;
                            var operatorData = {
                                top: jsonData.SourceDictionary[s].top,
                                left: jsonData.SourceDictionary[s].left,
                                opId: operatorId,
                                properties: {
                                    title: 'Source',
                                    //  inputs: {
                                    //     input_1: {
                                    //         label: 'Input 1',
                                    //     }
                                    //     },
                                    outputs: {
                                        output_1: {
                                            label: 'Output 1',
                                        }
                                    }
                                }
                            };

                            $('#example').flowchart('createOperator', operatorId, operatorData);
                            var source = "source";
                            // updateContainerVal(source, operatorId);
                        }
                    }


                    if (source_op != null) {
                        sop = source_op.split('_');
                        source_operatorI = parseInt(sop[1]) + 1;
                    }
                    //FIlter
                    source_op = null;
                    for (var f in jsonData.FilterDictionary) {
                        if (jsonData.FilterDictionary.hasOwnProperty(f)) {
                            //console.log(f + '   ' + jsonData.FilterDictionary[f].FilterName);
                            var operatorId = jsonData.FilterDictionary[f].FilterName;
                            source_op = operatorId;
                            var operatorData = {
                                top: jsonData.FilterDictionary[f].top,
                                left: jsonData.FilterDictionary[f].left,
                                opId: operatorId,
                                opId: operatorId,
                                properties: {
                                    title: 'Filter',
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
                            $('#example').flowchart('createOperator', operatorId, operatorData);
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    if (source_op != null) {
                        sop = source_op.split('_');
                        filter_operatorI = parseInt(sop[1]) + 1;
                    }
                    //Filter
                    source_op = null;
                    for (var t in jsonData.TargetDictionary) {
                        if (jsonData.TargetDictionary.hasOwnProperty(t)) {
                            //console.log(t + '   ' + jsonData.TargetDictionary[t].TargetName);
                            var operatorId = jsonData.TargetDictionary[t].TargetName;
                            source_op = operatorId;
                            var operatorData = {
                                top: jsonData.TargetDictionary[t].top,
                                left: jsonData.TargetDictionary[t].left,
                                opId: operatorId,
                                properties: {
                                    title: 'target',
                                    inputs: {
                                        input_1: {
                                            label: 'Input 1',
                                        }
                                    }
                                }
                            };

                            $('#example').flowchart('createOperator', operatorId, operatorData);
                            var source = "source";
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    if (source_op != null) {
                        sop = source_op.split('_');
                        target_operatorI = parseInt(sop[1]) + 1;
                    }
                    source_op = null;
                    for (var a in jsonData.AggregatorDictionary) {
                        if (jsonData.AggregatorDictionary.hasOwnProperty(a)) {
                            //console.log(a + '   ' + jsonData.AggregatorDictionary[a].AggregatorName);
                            var operatorId = jsonData.AggregatorDictionary[a].AggregatorName;
                            source_op = operatorId;
                            var operatorData = {
                                top: jsonData.AggregatorDictionary[a].top,
                                left: jsonData.AggregatorDictionary[a].left,
                                opId: operatorId,
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

                            $('#example').flowchart('createOperator', operatorId, operatorData);
                            var source = "source";
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    if (source_op != null) {
                        sop = source_op.split('_');
                        aggregator_operatorI = parseInt(sop[1]) + 1;
                    }
                    //-----------
                    source_op = null;
                    for (var a in jsonData.ExpressionDictionary) {
                        if (jsonData.ExpressionDictionary.hasOwnProperty(a)) {
                            //console.log(a + '   ' + jsonData.AggregatorDictionary[a].AggregatorName);
                            var operatorId = jsonData.ExpressionDictionary[a].ExpressionName;
                            source_op = operatorId;
                            var operatorData = {
                                top: jsonData.ExpressionDictionary[a].top,
                                left: jsonData.ExpressionDictionary[a].left,
                                opId: operatorId,
                                opId: operatorId,
                                properties: {
                                    title: 'Expression',
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

                            $('#example').flowchart('createOperator', operatorId, operatorData);
                            var source = "source";
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    if (source_op != null) {
                        sop = source_op.split('_');
                        expression_operatorI = parseInt(sop[1]) + 1;
                    }
                    //-----------
                    source_op = null;
                    for (var t in jsonData.JoinDictionary) {
                        if (jsonData.JoinDictionary.hasOwnProperty(t)) {
                            //console.log(t + '   ' + jsonData.JoinDictionary[t].JoinName);
                            var operatorId = jsonData.JoinDictionary[t].JoinName;
                            source_op = operatorId;
                            var operatorData = {
                                top: jsonData.JoinDictionary[t].top,
                                left: jsonData.JoinDictionary[t].left,
                                opId: operatorId,
                                properties: {
                                    title: 'Joiner',
                                    inputs: {
                                        input_1: {
                                            label: 'Input 1',
                                        },
                                        input_2: {
                                            label: 'Input 2',
                                        },
                                    },
                                    outputs: {
                                        output_1: {
                                            label: 'Output 1',
                                        }
                                    }
                                }
                            };

                            $('#example').flowchart('createOperator', operatorId, operatorData);
                            var source = "source";
                            // updateContainerVal(source, operatorId);
                        }
                    }
                    if (source_op != null) {
                        sop = source_op.split('_');
                        joiner_operatorI = parseInt(sop[1]) + 1;
                    }



                    for (var s in jsonData.SourceDictionary) {
                        if (jsonData.SourceDictionary.hasOwnProperty(s)) {
                            if (jsonData.SourceDictionary[s].ConnectedTo != '' && jsonData.SourceDictionary[s].toConnector != '') {
                                var linkData = {
                                    fromOperator: jsonData.SourceDictionary[s].SourceName,
                                    fromConnector: 'output_1',
                                    fromSubConnector: 0,
                                    toOperator: jsonData.SourceDictionary[s].ConnectedTo,
                                    toConnector: jsonData.SourceDictionary[s].toConnector,
                                    toSubConnector: 0
                                };
                                console.log(linkData);
                                $('#example').flowchart('addLink', linkData);
                            }
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    for (var s in jsonData.FilterDictionary) {
                        if (jsonData.FilterDictionary.hasOwnProperty(s)) {
                            console.log(jsonData.FilterDictionary[s]);
                            if (jsonData.FilterDictionary[s].ToSource != '' && jsonData.FilterDictionary[s].toConnector != '') {
                                var linkData = {
                                    fromOperator: jsonData.FilterDictionary[s].FilterName,
                                    fromConnector: 'output_1',
                                    fromSubConnector: 0,
                                    toOperator: jsonData.FilterDictionary[s].ToSource,
                                    toConnector: 'input_1', //chappi jsonData.FilterDictionary[s].toConnector,
                                    toSubConnector: 0
                                };
                                console.log(linkData);
                                $('#example').flowchart('addLink', linkData);
                            }
                            // updateContainerVal(source, operatorId);
                        }
                    }


                    for (var s in jsonData.AggregatorDictionary) {
                        if (jsonData.AggregatorDictionary.hasOwnProperty(s)) {
                            if (jsonData.AggregatorDictionary[s].ToSource != '' && jsonData.AggregatorDictionary[s].toConnector != '') {
                                var linkData = {
                                    fromOperator: jsonData.AggregatorDictionary[s].AggregatorName,
                                    fromConnector: 'output_1',
                                    fromSubConnector: 0,
                                    toOperator: jsonData.AggregatorDictionary[s].ToSource,
                                    toConnector: jsonData.AggregatorDictionary[s].toConnector,
                                    toSubConnector: 0
                                };

                                $('#example').flowchart('addLink', linkData);
                            }
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    for (var s in jsonData.ExpressionDictionary) {
                        if (jsonData.ExpressionDictionary.hasOwnProperty(s)) {
                            if (jsonData.ExpressionDictionary[s].ToSource != '' && jsonData.ExpressionDictionary[s].toConnector != '') {
                                var linkData = {
                                    fromOperator: jsonData.ExpressionDictionary[s].ExpressionName,
                                    fromConnector: 'output_1',
                                    fromSubConnector: 0,
                                    toOperator: jsonData.ExpressionDictionary[s].ToSource,
                                    toConnector: jsonData.ExpressionDictionary[s].toConnector,
                                    toSubConnector: 0
                                };

                                $('#example').flowchart('addLink', linkData);
                            }
                            // updateContainerVal(source, operatorId);
                        }
                    }

                    for (var s in jsonData.JoinDictionary) {
                        if (jsonData.JoinDictionary.hasOwnProperty(s)) {
                            if (jsonData.JoinDictionary[s].ToSource != '' && jsonData.JoinDictionary[s].toConnector != '') {
                                var linkData = {
                                    fromOperator: jsonData.JoinDictionary[s].JoinName,
                                    fromConnector: 'output_1',
                                    fromSubConnector: 0,
                                    toOperator: jsonData.JoinDictionary[s].ToSource,
                                    toConnector: jsonData.JoinDictionary[s].toConnector,
                                    toSubConnector: 0
                                };

                                $('#example').flowchart('addLink', linkData);
                            }

                            // updateContainerVal(source, operatorId);
                        }
                    }
                }


            },
            failure: function (result) {
                $('.spinner').css('display', 'none');
                alert('Failed');
            }


        });
        
        setTimeout(() => {
            console.log('End: ' + x);
            resolve(x);
        }, sec * 200);
    });
}

async function SerialFlow(data) {

    let result1 = await myPromiseDelSrc(1, 1);
    let result2 = await myPromiseDelAgr(2, 2);
    let result3 = await myPromiseDelJoiner(3, 3);
    let result4 = await myPromiseDelFilter(4, 4);
    let result5 = await myPromiseDelTrg(5, 5);
    let result6 = await myPromiseDelTrans(6, 6);
    let result7 = await myPromiseDelLinks(7, 7);
    let result8 = await myPromiseload(8, 8, data);
    $('.spinner').css('display', 'none');

}





var activeContainerId = "";
var source_operatorI = 1;
var joiner_operatorI = 1;
var filter_operatorI = 1;
var expression_operatorI = 1;
var aggregator_operatorI = 1;
var target_operatorI = 1;
var filter_row_count = 0;
//----------------Source Model code-----------
$('#srcModelSave').on('click', function () {
    var x = $('#SourceOutputFlags').val();
    var model = {
        ConnectionName: $('#connectionList').val(),
        SourceOutputFlags: $('#SourceOutputFlags').val(),
        TableName: $('#sourceTableList').val(),
        SourceName: activeContainerId

    }
    console.log('Source Output FLags:' + $('#SourceOutputFlags').val());
    $.ajax({
        url: "Home/SourceSaveChanges",
        type: "Post",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(model), //if you need to post Model data, use this
        success: function (result) {
            $("#partial").html(result);
        }
    });
});
//---------------Source Table Checkbox Handling--------------
$('#sourceTable').on('change',
    'input[type="checkbox"]',
    function (e) {
        var chkVal = $(this).prop('value');
        if (chkVal === 'true') {
            $(this).prop('value', 'false').removeAttr('checked');
        } else {
            $(this).prop('value', 'true').prop('checked', 'checked');
            //$('#sourceTable').find("[data-id= 0]").prop('value', 'true').prop('checked', 'checked');
        }
        updateSourceOutputFlag();
    });

function updateSourceOutputFlag() {
    var checkedAssetIds = [];
    $('#sourceTable input[type="checkbox"]').each(function (idx, val) {
        if ($(this).prop('checked')) {
            checkedAssetIds.push($(this).data('id'));
        }
    });
    $('#SourceOutputFlags').val(checkedAssetIds);

}
//---------------Source Table Checkbox Handling--------------
function getTableHeader(connectionName, tableName) {

    //$('.spinner').css('display', 'block');
    $.getJSON('/home/GetTableHeader',
        { connName: connectionName, tableName: tableName, containerId: activeContainerId },
        function (data) {

            if ($('#connectionList').val() !== '' && $('#sourceTableList').val() !== '') {
                $('#sourceTableBody').empty();
                var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
                //alert(data);
                var trHTML = '';
                for (var i = 0; i < jsonData.length; i++) //The json object has lenght
                {
                    var isChecked = ' value = "false"';
                    if (jsonData[i].OutputFlag === true) {
                        isChecked = ' value = "true" checked="checked"';
                    }

                    trHTML += '<tr>' +
                        '<td>' +
                        jsonData[i].ColumnName +
                        '</td>' +
                        '<td> <div class="custom-control custom-checkbox"> ' +
                        '<input type="checkbox" class="checkbox sourceRow" id="' +
                        jsonData[i].ColumnId +
                        '"  data-id = "' +
                        jsonData[i].ColumnId +
                        '" ' + isChecked + '>' +
                        '</div> </td> </tr>';
                }
                $('#sourceTableBody').append(trHTML);
                updateSourceOutputFlag();
            }


        }).fail(function (jqXHR, textStatus, errorThrown) {

            alert('Error getting tableLists!');
        });

    // $('.spinner').css('display', 'none');
}

function getTables(connName, selectedTableName) {
    console.log('Get Table COnnection Calling');
    console.log(selectedTableName);
    $.getJSON('/home/GetTable', { connName: connName }, function (data) {

        $('#sourceTableList option').remove();
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
        $('#sourceTableList').append('<option>' + '--Select Table Name--' + '</option>');
        if (jsonData !== null) {
            for (var i = 0; i < jsonData.length; i++) //The json object has lenght
            {
                var tabVal = jsonData[i];
                $('#sourceTableList').append('<option value=' +
                    tabVal + '>' + tabVal + '</option>');
            }
        }
        //Set Selected Value
        if (selectedTableName != '') {
            $('#sourceTableList').val(selectedTableName);
            //var ret = $('#sourceTableList').val()
            console.log(activeContainerId + '-' + selectedTableName);
        }
        return selectedTableName;

    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting source table names!');
    });
}

function reOpenProperties(TableName) {

}

function getConnection() {
    $.getJSON('/home/GetConnections', { containerId: activeContainerId }, function (data) {
        //-----------Set Source Window Config-----------

        $('#connectionList option').remove();
        $('#sourceTableList option').remove();
        $('#sourceTableBody').empty();
        $('#sourceTableName').empty();   
        $('#sourceTableName').text('Table Name');  
        console.log('Connection Calling');
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
        var conn = jsonData.Connection;
        var sourceModel = jsonData.SourceModel;
        $('#connectionList').append('<option >' + '--Select Connection Name--' + '</option>');
        for (var i = 0; i < conn.length; i++) //The json object has lenght
        {
            //conn[i]; //You are in the current object
            $('#connectionList').append('<option value=' +
                conn[i] + '>' + conn[i] + '</option>');
        }
        //Set default value
        $('#sourceTableList option').remove();
        $('#sourceTableList').append('<option>' + '--Select Table Name--' + '</option>');
        //-----------Set Source Window Config-----------
        if (sourceModel.TableName != '') {
            $('#connectionList').val(sourceModel.ConnectionName);
            $('#sourceTableName').text(sourceModel.TableName);
            
            if (sourceModel.TableName === '--Select Table Name--') {
                $('#sourceTableName').text('Table Name');   
            }
               
            console.log($('#sourceTableName').val() + '------' + sourceModel.TableName);
            console.log(sourceModel.ConnectionName + '--' + sourceModel.TableName);
            let res = getTables($('#connectionList').val(), sourceModel.TableName);
            console.log(res);
            getTableHeader($('#connectionList').val(), sourceModel.TableName);
            
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting categories!');
    });
}

$('#connectionList').on('change',
    function () {
        $('#tableList option').empty();
        $('#sourceTableBody').empty();
        console.log('Connection List Change');
        //--------------Table Names--------------
        getTables($('#connectionList').val(), '');
        //--------------Table Names--------------
    });

$('#sourceTableList').on('change',
    function () {
        //$('#tableList option').empty();
        //--------------Table Header--------------
        getTableHeader($('#connectionList').val(), $('#sourceTableList').val());
        //--------------Table Header--------------
    });
function getProjectMappingDetail() {
    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            return jsonData;
        });
}
$('#create_source').click(function () {
    //var result = getProjectMappingDetail();
    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            if (jsonData.Project == -1 && jsonData.Mapping == -1) {
                alert('Please select a Project/ Mapping');
            }
            else {
                var operatorId = 'source_' + source_operatorI;
                var operatorData = {
                    top: 60,
                    left: 500,
                    opId: operatorId,
                    properties: {
                        title: 'Source',
                        //  inputs: {
                        //     input_1: {
                        //         label: 'Input 1',
                        //     }
                        //     },
                        outputs: {
                            output_1: {
                                label: 'Output 1',
                            }
                        }
                    }
                };
                source_operatorI++;
                $('#example').flowchart('createOperator', operatorId, operatorData);
                var source = "source";
                updateContainerVal(source, operatorId);
                Logs(operatorId + " is created <br />");
            }

        });
});
//----------------Source Model code-----------

//----------------Aggregator Model code-----------



//----------------Aggregator Model code-----------

//----------------Join Model code-----------
function joinerSettings(active) {
    $.getJSON('/home/JoinerSettings', { containerId: active }, function (data) {
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
        var sourceModel1 = jsonData.SourceModel1;
        var sourceModel2 = jsonData.SourceModel2;
        var srcMod1SelCol = jsonData.SourceModel1SelectedColumn;
        var srcMod2SelCol = jsonData.SourceModel2SelectedColumn;
        var joinType = jsonData.JoinType;
        //$('#').empty();
        //$('#aggregator-table-name').text(sourceModel.ConnectionName + ' -> ' + sourceModel.TableName);

        $('#join-source-1-tb').empty();
        $('#join-source-2-tb').empty();
        $('#select-source-1-col').empty();
        $('#select-source-2-col').empty();
        $('#select-join-type').val('');
        //Set source 1
        $('#select-source-1-col').append('<option >' + '--Select Column Name--' + '</option>');
        $('#select-source-2-col').append('<option >' + '--Select Column Name--' + '</option>');

        var trHtml = '';
        var i = '';
        var isChecked = '';
        for (i = 0; i < sourceModel1.InputModel.length; i++) //The json object has length
        {
            isChecked = ' value = "false" ';
            if (sourceModel1.InputModel[i].OutputFlag === true) {
                isChecked = ' value = "true" checked = "checked" ';
                $('#select-source-1-col').append('<option value = "' + sourceModel1.InputModel[i].ColumnName + '" > ' + sourceModel1.InputModel[i].ColumnName + '</option>');
            }
            trHtml += '<tr>' +
                '<td>' +
                sourceModel1.InputModel[i].ColumnName +
                '</td>' +
                '<td> <div class="custom-control custom-checkbox"> ' +
                '<input type="checkbox" class="checkbox " disabled  ' + isChecked + ' >' +
                '</div> </td> </tr>';
        }
        $('#join-source-1-tb').append(trHtml);
        $('#select-source-1-col').val(srcMod1SelCol);
        //Set source 2
        trHtml = '';
        for (i = 0; i < sourceModel2.InputModel.length; i++) //The json object has lenght
        {
            isChecked = ' value = "false"';
            if (sourceModel2.InputModel[i].OutputFlag === true) {
                isChecked = ' value = "true" checked="checked" ';
                $('#select-source-2-col').append('<option value = "' + sourceModel2.InputModel[i].ColumnName + '" > ' + sourceModel2.InputModel[i].ColumnName + '</option>');
            }
            trHtml += '<tr>' +
                '<td>' +
                sourceModel2.InputModel[i].ColumnName +
                '</td>' +
                '<td> <div class="custom-control custom-checkbox"> ' +
                '<input type="checkbox" class="checkbox " disabled ' + isChecked + ' >' +
                '</div> </td> </tr>';
        }
        $('#join-source-2-tb').append(trHtml);
        $('#select-source-2-col').val(srcMod2SelCol);
        $('#select-join-type').val(joinType);


    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting aggregation settings!');
    });
}

$('#joinerModelSave').on('click', function () {
    var model = {
        SourceModel1SelectedColumn: $('#select-source-1-col').val(),
        SourceModel2SelectedColumn: $('#select-source-2-col').val(),
        JoinName: activeContainerId,
        JoinType: $('#select-join-type').val()
    }

    $.ajax({
        url: "Home/JoinerSaveChanges",
        type: "Post",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(model), //if you need to post Model data, use this
        success: function (result) {
            $("#partial").html(result);
            Logs(activeContainerId + " changes saved <br />");
        }
    });
});
$('#create_joiner').click(function () {

    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            if (jsonData.Project == -1 && jsonData.Mapping == -1) {
                alert('Please select a Project/ Mapping');
            }
            else {
                var operatorId = 'joiner_' + joiner_operatorI;
                var operatorData = {
                    top: 60,
                    left: 500,
                    opId: operatorId,
                    properties: {
                        title: 'Joiner',
                        inputs: {
                            input_1: {
                                label: 'Input 1',
                            },
                            input_2: {
                                label: 'Input 2',
                            },
                        },
                        outputs: {
                            output_1: {
                                label: 'Output 1',
                            }
                        }
                    }
                };

                joiner_operatorI++;

                $('#example').flowchart('createOperator', operatorId, operatorData);
                var source = "joiner";
                //alert(source + ' - ' + operatorId);
                updateContainerVal(source, operatorId);
                Logs(operatorId + " is created <br />");
            }
        });

});
//----------------Join Model code-----------

//----------------Target Model code-----------
function targetSettings(activeContainerId) {
    $.getJSON('/home/TargetSettings', { containerId: activeContainerId }, function (data) {
        var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json

        $('#target-table-body').empty();

        var joinModel = jsonData.JoinModel;
        var filterModel = jsonData.FilterModel;
        var agrModel = jsonData.AggregatorModel;
        var connectedFrom = jsonData.ConnectedFrom;
        var inputModel = jsonData.InputModel;
        var obj = '';
        var trHtml = '';
        var thHtml = '<tr> <th scope = "col" > Columns</th> </tr >';
        var i = 0;
        $('#target-table-header').empty();
        if (connectedFrom.includes("joiner")) {
            $('#target-table-header').append(thHtml);
            trHtml = '';
            for (i = 0; i < inputModel.length; i++) //The json object has length
            {
                trHtml += '<tr>' +
                    '<td>' +
                    inputModel[i].ColumnName +
                    '</td> </tr>';
            }
            $('#target-table-body').append(trHtml);
            Logs(activeContainerId + " is initialized <br />");
            /*trHtml = '';
            for (i = 0; i < joinModel.SourceModel2.InputModel.length; i++) //The json object has length
            {
                trHtml += '<tr>' +
                    '<td>' +
                    joinModel.SourceModel2.InputModel[i].ColumnName +
                    '</td> </tr>';
            }
            $('#target-table-body').append(trHtml);*/
        } else if (connectedFrom.includes("aggregator")) {
            $('#target-table-header').append(thHtml);
            obj = agrModel;
            trHtml = '';
            for (i = 0; i < agrModel.InputModel.length; i++) //The json object has lenght
            {
                if (agrModel.InputModel[i].OutputFlag === true) {
                    trHtml += '<tr>' +
                        '<td>' +
                        agrModel.InputModel[i].ColumnName + '</td> </tr>';
                }

            }
            $('#target-table-body').append(trHtml);
            Logs(activeContainerId + " is initialized <br />");
        } else if (connectedFrom.includes("filter")) {
            thHtml = '<tr> ' +
                '<th scope = "col" > Columns</th>' +
                '<th scope = "col" > Operator</th>' +
                '<th scope = "col" > Value</th>' +
                '<th scope = "col" > Condition</th>' +
                ' </tr >';
            $('#target-table-header').append(thHtml);
            obj = filterModel;
            trHtml = '';
            for (i = 0; i < obj.SelectedFilterModel.length; i++) //The json object has lenght
            {
                trHtml += '<tr>' +
                    '<td>' + obj.SelectedFilterModel[i].ColumnName + '</td>' +
                    '<td>' + obj.SelectedFilterModel[i].FilterOperator + '</td>' +
                    '<td>' + obj.SelectedFilterModel[i].FilterValue + '</td>' +
                    '<td>' + obj.SelectedFilterModel[i].FilterCondition + '</td>' +
                    '</tr>';

            }
            $('#target-table-body').append(trHtml);
            Logs(activeContainerId + " is initialized <br />");
        } else if (connectedFrom.includes("expression")) {
            thHtml = '<tr> ' +
                '<th scope = "col" > Columns</th>' +
                '<th scope = "col" > Data Type</th>' +
                ' </tr >';
            $('#target-table-header').append(thHtml);
            obj = inputModel;
            trHtml = '';
            for (i = 0; i < obj.length; i++) //The json object has lenght
            {
                trHtml += '<tr>' +
                    '<td>' + obj[i].ColumnName + '</td>' +
                    '<td>' + obj[i].toDataType + '</td>' +
                    '</tr>';

            }
            $('#target-table-body').append(trHtml);
            Logs(activeContainerId + " is initialized <br />");
        }


    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error getting aggregation settings!');
    });
}

$('#create_target').click(function () {
    $.getJSON('/home/GetProjectMappingDetail',
        {},
        function (data) {
            var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json
            if (jsonData.Project == -1 && jsonData.Mapping == -1) {
                alert('Please select a Project/ Mapping');
            }
            else {
                var operatorId = 'target_' + target_operatorI;
                var operatorData = {
                    top: 60,
                    left: 500,
                    opId: operatorId,
                    properties: {
                        title: 'target',
                        inputs: {
                            input_1: {
                                label: 'Input 1',
                            }
                        }
                    }
                };
                target_operatorI++;

                $('#example').flowchart('createOperator', operatorId, operatorData);
                var source = "target";
                //alert(source + ' - ' + operatorId);
                updateContainerVal(source, operatorId);
                Logs(operatorId + " is created <br />");
            }
        });



});
//----------------Target Model code-----------

//----------------Common---------------------
function updateContainerVal(containerName, containerId) {
    $.getJSON('/home/UpdateContainerVal', { container: containerName, contId: containerId }, function (data) {
    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error setting New Container!');
    });
}

$(document).ready(function () {
    // Dropdown list change event.
    var data = {
        operators: {
            operator1: {
                top: 20,
                left: 20,
                properties: {
                    title: 'Operator 1',
                    inputs: {},
                    outputs: {
                        output_1: {
                            label: 'Output 1',
                        }
                    }
                }
            },
            operator2: {
                top: 80,
                left: 300,
                properties: {
                    title: 'Operator 2',
                    inputs: {
                        input_1: {
                            label: 'Input 1',
                        },
                        input_2: {
                            label: 'Input 2',
                        },
                    },
                    outputs: {}
                }
            },
        }
    };
    // Apply the plugin on a standard, empty div...
    $('#example').flowchart({
        //data: data
    });
});
var operatorI = 0;
$('#create_operator').click(function () {
    var operatorId = 'created_operator_' + operatorI;
    var operatorData = {
        top: 60,
        left: 500,
        opId: operatorId,
        properties: {
            title: 'Operator ' + (operatorI + 3),
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

    operatorI++;

    $('#example').flowchart('createOperator', operatorId, operatorData);
});
$('#delete_selected_button').click(function () {
    $('#example').flowchart('deleteSelected');
});

$('#delete_selected_button').click(function () {
    $('#example').flowchart('deleteSelected');
});
$('#properties_selected_button').click(function () {
    $('#example').flowchart('propertiesSelected');
});

function getOperatorlinkdata(linkdata) {
    var operatorlinkdata = linkdata;
    var fromOperator = operatorlinkdata["fromOperator"];
    var toOperator = operatorlinkdata["toOperator"];
    var toConnector = operatorlinkdata["toConnector"];
    $.getJSON('/home/UpdateContainerLinks', { fromContainer: fromOperator, toContainer: toOperator, toConnector: toConnector }, function (data) {
        //var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json


    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error setting New Container!');
    });
    //console.log(linkdata);
}

function DeleteOperatorslink(linkdata) {
    var operatorlinkdata = linkdata;
    var fromOperator = operatorlinkdata["fromOperator"];
    var toOperator = operatorlinkdata["toOperator"];
    var toConnector = operatorlinkdata["toConnector"];
    $.getJSON('/home/DeleteContainerLinks', { fromContainer: fromOperator, toContainer: toOperator, toConnector: toConnector }, function (data) {
        //var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json


    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error setting New Container!');
    });
    //console.log(linkdata);
}

function DeleteOperatorfrombackend(operatorId) {
    var operatorId = operatorId;
    $.getJSON('/home/DeleteNode', { operatorId: operatorId }, function (data) {
        //var jsonData = jQuery.parseJSON(JSON.stringify(data)); //This converts the string to json


    }).fail(function (jqXHR, textStatus, errorThrown) {
        alert('Error setting New Container!');
    });
    //console.log(linkdata);
}

function propertiesOperator(id) {
    var operatorid = id;
    var node_title = id.substr(0, id.indexOf('_'));
    activeContainerId = operatorid;
    Logs(activeContainerId + " properrties opened <br />");
    if (node_title == 'source') {

        $("#sourcewindow").modal();
        //--------------Connection Names--------------
        //set source model values
        getConnection();
        //--------------Connection Names--------------
        //updateSourceOutputFlag();
    }
    if (node_title == 'joiner') {
        joinerSettings(activeContainerId);
        $("#joinerwindow").modal();
    }

    if (node_title == 'aggregator') {
        //set aggregator model values
        aggregatorSettings(activeContainerId);
        $("#aggregatorwindow").modal();
    }
    if (node_title == 'filter') {
        //set aggregator model values
        filterSettings(activeContainerId);
        $("#filterwindow").modal();
    }
    if (node_title == 'target') {
        targetSettings(activeContainerId);
        $("#targetwindow").modal();
    }
    if (node_title == 'expression') {
        expressionSettings(activeContainerId);
        $("#expressionwindow").modal();
    }

}

$('#newBtn').on('click',
    function () {
        $.ajax({
            url: "Home/NewModel",
            type: "Post",
            success: function (result) {
                $("#partial").html(result);
                Logs("New Mapping is initialized <br />");
            }
        });

    });
$('#excelBtn').on('click',
    function () {
        $.ajax({
            url: "Home/ExcelConnection",
            type: "Post",
            data: $("form").serialize(), //if you need to post Model data, use this
            success: function (result) {
                $("#partial").html(result);
                Logs("Excel Connection is initialized <br />");
            }
        });
    });
$('#executeJob').on('click',
    function () {
        Logs("Execute Mapping <br />");
        $('.spinner').css('display', 'block');
        $.ajax({
            url: "Home/ExecuteJob",
            type: "Post",
            data: $("form").serialize(), //if you need to post Model data, use this
            success: function (result) {

                if (result == null) {

                    $("#partial").html(result);
                    Logs(result + "<br />");
                    $('.spinner').css('display', 'none');
                }
                else {
                    Logs(result+ "<br />");
                    $('.spinner').css('display', 'none');
                    // alert(result);
                }
            },
            failure: function (result) {
                Logs(result + "<br />");
                $('.spinner').css('display', 'none');
               // alert('mapping is failed');
            }
        });
    });

$('#sqlBtn').on('click',
    function () {
        Logs("SQL Connection is initialized <br />");
        $('.spinner').css('display', 'block');
        document.getElementById("sqlServerLi").classList.remove('open');

        var model = {
            ServerName: $('#sqlServerServerName').val(),
            DbName: $('#sqlServerDatabaseName').val(),
            UserName: $('#sqlServerUserName').val(),
            Password: $('#sqlServerPassword').val()
        }
        $.ajax({
            url: "Home/SqlServerConnection",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(model), //if you need to post Model data, use this
            success: function (result) {
                if (result === "Failure") {
                    $('.spinner').css('display', 'none');
                    alert("Connection to SQL Server Failed");

                }
                else {
                    // $("#sqlServerLi").removeClass("open");
                    $("#partial").html(result);
                    $('.spinner').css('display', 'none');
                }

            },
            failure: function (result) {
                $('.spinner').css('display', 'none');
                alert('Failed');
            }
        });

    });



$('#validate').on('click',
    function () {
        Logs("Mapping Validation is initiated <br />");
        $('.spinner').css('display', 'block');
        $.ajax({
            url: "Home/Validate",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            //  data: JSON.stringify(model), //if you need to post Model data, use this
            success: function (result) {

                $('.spinner').css('display', 'none');
                if (result = null) {
                    result = "Workflow is valid.";
                }
                var res = jQuery.JSON.stringify(result);
                Logs("Validation Result is: " + res + " <br />");

            },
            failure: function (result) {
                alert('Failed');
                $('.spinner').css('display', 'none');
            }


        });

    });
//----------------Common---------------------a
function Logs(a) {
    //var a = "Cloning into 'ticgit'...<br /> remote: Reusing existing pack: 1857, done.<br /> remote: Total 1857 (delta 0), reused 0 (delta 0) < br /> Receiving objects: 100 % (1857 / 1857), 374.35 KiB | 268.00 KiB/ s, done.<br /> Resolving deltas: 100 % (772 / 772), done.<br /> Checking connectivity... done.Cloning into 'ticgit'...<br />            remote: Reusing existing pack: 1857, done.<br /> remote: Total 1857 (delta 0), reused 0 (delta 0) < br />  Receiving objects: 100 % (1857 / 1857), 374.35 KiB | 268.00 KiB/ s, done.<br />            remote: Reusing existing pack: 1857, done.remote: Total 1857 (delta 0), reused 0 (delta 0)  Receiving objects: 100 % (1857 / 1857), 374.35 KiB | 268.00 KiB/ s, done.Resolving deltas: 100 % (772 / 772), done.git clone https://github.com/schacon/ticgit            Cloning into 'ticgit'...remote: Reusing existing pack: 1857, done.remote: Total 1857 (delta 0), reused 0 (delta 0) Receiving objects: 100 % (1857 / 1857), 374.35 KiB | 268.00 KiB/ s, done.Resolving deltas: 100 % (772 / 772), done.";s
    $("#logsData").append(a);

}


$('#save').on('click',
    function () {
        Logs("Save Mapping Changes <br />");
        $('.spinner').css('display', 'block');
        var model = new Array();
        var elements = document.getElementsByClassName("flowchart-operator flowchart-default-operator ui-draggable");
        for (var i = 0, len = elements.length; i < len; i++) {
            if (elements[i].id.includes('source_') || elements[i].id.includes('target_') || elements[i].id.includes('aggregator_') || elements[i].id.includes('joiner_')
                || elements[i].id.includes('filter_') || elements[i].id.includes('expression_')) {
                var obj = {
                    OperatorId: elements[i].id,
                    top: parseInt(elements[i].style.top),
                    left: parseInt(elements[i].style.left)
                }
                model.push(obj);
            }
        }



        $.ajax({
            url: "Home/Save",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(model), //if you need to post Model data, use this
            success: function (result) {

                $('.spinner').css('display', 'none');
                alert(result);
                $('#html1').jstree(true).refresh();

            },
            failure: function (result) {
                alert('Failed');
                $('.spinner').css('display', 'none');
            }


        });

    });


$('#load').on('click',
    function () {
        Logs("Mapping is loaded <br />");
        $('.spinner').css('display', 'block');
        $.ajax({
            url: "Home/Load",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(model), //if you need to post Model data, use this
            success: function (data) {



                $('.spinner').css('display', 'none');
                var jsonData = jQuery.parseJSON(JSON.stringify(data))


            },
            failure: function (result) {
                alert('Failed');
                $('.spinner').css('display', 'none');
            }


        });

    });

