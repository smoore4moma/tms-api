var w = 1000,
    h = 400;

var circleWidth = 5;

var palette = {

    "gray": "#708284",
    "mediumgray": "#536870",
    "pink": "#C61C6F",
    "blue": "#2176C7",
    "yellowgreen": "#738A05",
    "gold": "#FFD700"
}


//var url = "http://localhost/tms-api/objects/{some object id}" + "?token=";
var url = "../Scripts/clients/object.json";

var nodes = [];

d3.json(url, function (json) {

    var nodeParent = { name: json.objects[0].title };
    nodes.push(nodeParent);

    console.log(nodes);

    for (var i = 0; i < json.objects[0].exhibitions.resultsCount; i++) {

        var exhTitle = json.objects[0].exhibitions.exhibitions[i].exhibitionTitle
        exhTitle = exhTitle.replace(/'/g, '\'');
           
        var node = { name: exhTitle, target: [0], type: "exhibition" };
        nodes.push(node);
    }

    for (var i = 0; i < json.objects[0].terms.resultsCount; i++) {

        var node = { name: json.objects[0].terms.terms[i].term, target: [0], type: "term" };
        nodes.push(node);
    }

    var links = [];

    for (var i = 0; i < nodes.length; i++) {

        if (nodes[i].target !== undefined) {
            for (var x = 0; x < nodes[i].target.length; x++) {
                links.push({
                    source: nodes[i],
                    target: nodes[nodes[i].target[x]]
                })
            }
        }
    }

    var myChart = d3.select('#chart')
            .append('svg')
            .attr('width', w)
            .attr('height', h)

    var force = d3.layout.force()
        .nodes(nodes)
        .links([])
        .gravity(0.3)
        .charge(-1000)
        .size([w, h])

    var link = myChart.selectAll('line')
        .data(links).enter().append('line')
        .attr('stroke', palette.gray)

    var node = myChart.selectAll('circle')
        .data(nodes).enter()
        .append('g')
        .call(force.drag);

    node.append('circle')
        .attr('cx', function (d) { return d.x; })
        .attr('cy', function (d) { return d.y; })
        .attr('term', function (d) { return d.type; })
        .attr('r', circleWidth)
        .attr('fill', function (d, i) {
            if (i == 0) { return palette.blue }
            else if (d.type == "term") { return palette.gold }
            else if (d.type == "exhibition") { return palette.pink }
            else { return palette.gray }
        })

    node.append('text')
        .text(function (d) { return d.name })
        .attr('font-family', 'Helvetica')
        .attr('fill', function (d, i) {
            if (i > 0) { return palette.mediumgray }
            else { return palette.yellowgreen }
        })
        .attr('x', function (d, i) {
            if (i > 0) { return circleWidth + 4 }
            else { return circleWidth - 15 }
        })
        .attr('y', function (d, i) {
            if (i > 0) { return circleWidth }
            else { return 8 }
        })
        .attr('text-anchor', function (d, i) {
            if (i > 0) { return 'start' }
            else { return 'end' }
        })
        .attr('font-size', function (d, i) {
            if (i > 0) { return '1em' }
            else { return '1.8em' }
        })

    force.on('tick', function (e) {
        node.attr('transform', function (d, i) {
            return 'translate(' + d.x + ', ' + d.y + ')';
        })

        link
            .attr('x1', function (d) { return d.source.x })
            .attr('y1', function (d) { return d.source.y })
            .attr('x2', function (d) { return d.target.x })
            .attr('y2', function (d) { return d.target.y })
    })

    force.start();

});


