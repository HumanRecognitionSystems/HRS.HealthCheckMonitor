$(function () {
    window.setInterval(gatherData, 1000);
});

function gatherData() {
    $.getJSON("/monitor-api", function (data) {
        var hc = data.siteHealth;
        $("#name").val(hc.name);
        $("#status").val(hc.status);
        $("#statusStarted").val(date(hc.currentStatusStarted).getTime());
        $("#statusCount").val(hc.currentStatusCount);
        $("#lastDate").val(hc.lastEvaluationDate);
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
        data.push(entries[c]);
    }
    $("#entries").bootstrapTable({
        data: data
    });
}