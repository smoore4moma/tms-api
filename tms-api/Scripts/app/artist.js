var w = 1000,
    h = 600;

var circleWidth = 5;

var palette = {

    "gray": "#708284",
    "mediumgray": "#536870",
    "pink": "#C61C6F",
    "blue": "#2176C7",
    "yellowgreen": "#738A05"
}

var tooltip = d3.select('body').append('div')
       .attr('class', 'tooltip')

//var url = "http://localhost/tms-api/artists/{some artist id}" + "?token=";
var url = "../Scripts/clients/artist_24505.json";

var nodes = [];

d3.json(url, function (json) {

    var nodeParent = { name: json.displayName };
    nodes.push(nodeParent);
  
    for (var i = 0; i < json.objects.length; i++) {

        var node = { name: json.objects[i].title, target: [0], id: json.objects[i].objectID }; 
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
                .attr('id', function (d) { return 'm_' + d.id; })
                .attr('r', circleWidth)
                .attr('fill', function (d, i) {
                    if (i > 0) { return palette.pink }
                    else { return palette.blue }
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
                    if (i > 0) { return 'beginning' }
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

            d3.selectAll('circle')
        .on('click', function (d) {

            //var object_url = "http://localhost/tms-api/objects/" + this.id.replace('m_', '') + "?token=";
            var object_url = "../Scripts/clients/objects_" + this.id.replace('m_', '') + ".json";

            d3.json(object_url, function (json_obj) {

                d3.select('#m_' + json_obj.objects[0].objectID)

                tooltip.transition()
                       .style('opacity', .95)

                var onView;

                if (json_obj.objects[0].onView == 1)
                { onView = 'Yes' }
                else
                { onView = 'No' }

                tooltip.html("<table><tr><td id='tooltipImage'><img src='"
                    + json_obj.objects[0].thumbnail + "' /></td><td id='tooltipData'><span style='font-size: bigger;font-weight:bold'>Title: </span>"
                    + json_obj.objects[0].title + "<br><strong>Artist: </strong>"
                    + json_obj.objects[0].displayName + "<br><strong>Biography: </strong>"
                    + json_obj.objects[0].displayDate + "<br><strong>Department: </strong>"
                    + json_obj.objects[0].department + "<br><strong>Classification: </strong>"
                    + json_obj.objects[0].classification + "<br><strong>Medium: </strong>"
                    + json_obj.objects[0].medium + "<br><strong>Dimensions: </strong>"
                    + json_obj.objects[0].dimensions + "<br><strong>Creditline: </strong>"
                    + json_obj.objects[0].creditLine + "<br><strong>On view: </strong>"
                    + onView + "</td></tr></table>")
                    .style('left', (d.x) + 'px')
                    .style('top', (d.y) + 'px')
                    .style('visibility', 'visible')


            });



        })
        .on("mouseout", function (d) {
            tooltip.transition()
                .duration(500)
                .style("opacity", 0);
        });



            force.start();

        });


