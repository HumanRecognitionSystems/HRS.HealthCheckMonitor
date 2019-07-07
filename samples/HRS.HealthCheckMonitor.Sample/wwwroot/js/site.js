$(function () {
    $("button.state").click(function () {
        var path = $(this).data("checked");
        $.post("api/state/" + path);
    });

    $("#entries").bootstrapTable();
    window.setInterval(gatherData, 1000);
});

function gatherData() {
    $.getJSON("/monitor-api", function (data) {
        var hc = data.siteHealth;

        var cls = clsForStatus(hc.status);

        $("#icon").removeClass().addClass("fa fa-" + cls);

        $("#name").val(hc.name);
        $("#status").val(hc.status);
        $("#statusStarted").val(moment(hc.currentStatusStarted).format("HH:mm:ss"));
        $("#statusCount").val(hc.currentStatusCount);
        $("#lastDate").val(moment(hc.lastEvaluationDate).format("HH:mm:ss"));
        $("#duration").val(hc.evaluationDuration);
        setTable(hc.entries);
    });
}

function setTable(entries) {
    if (entries == null) {
        return;
    }
    var data = [];
    for (var c in entries) {
        entries[c].name = c;
        entries[c].status = " <i class=\"fa fa-" + clsForStatus(entries[c].status) + "\"></i> " + entries[c].status; 
        data.push(entries[c]);
    }
    $("#entries").bootstrapTable("load", data);
    for (var d in data) {
    }
}

function clsForStatus(status) {
    var cls = "slash";
    switch (status) {
        case "Offline": cls = "wifi"; break;
        case "Healthy": cls = "check-circle text-success"; break;
        case "Degraded": cls = "exclamation-circle text-warning"; break;
        case "Unhealthy": cls = "times-circle text-danger"; break;
    }

    return cls;
}