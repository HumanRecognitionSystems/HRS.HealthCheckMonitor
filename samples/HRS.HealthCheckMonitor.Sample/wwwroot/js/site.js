$(function () {
    window.setInterval(gatherData, 1000);
});

function gatherData() {
    $.getJSON("/monitor-api", function (data) {
        var hc = data.siteHealth;
        $("#name").val(hc.name);
        $("#status").val(hc.status);
        setTable(hc.entries);
    });
}

function setTable(entries) {
    $("#entries").bootstrapTable({
        data: entries
    });
}