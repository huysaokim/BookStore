//rate
var result = 0;
var rater = document.getElementById('rate');
var stars = Array.from(rater.children);
rater.addEventListener('touchmove', raterEnd);
stars.forEach(function (item) {
    item.addEventListener('mousemove', rateStar.bind(null, item, showResult));

});



function raterEnd(e) {
    e.preventDefault();
    e.stopPropagation();
    var changedTouch = e.changedTouches[0];
    var elem = document.elementFromPoint(changedTouch.clientX, changedTouch.clientY);
    endElem = elem;
    rateStar(elem, showResult);
}


function rateStar(ratedItem, callback) {
    if (stars.includes(ratedItem)) {
        result = parseInt(ratedItem.dataset.score);
        stars.forEach(function (item) {
            var position = parseInt(item.dataset.score);
            if (position === 0) {
                item.style.color = "#ccc";
            } else if (position <= result) {
                item.style.color = "#dbcc8f";
            } else {
                item.style.color = "#ccc";
            }
        });
    }
    callback();
}
function showResult() {
    document.getElementById('result').value = result;
    document.getElementById('result_DP').innerHTML = "Rank: " + getRank(result);
}

function getRank(result) {
    switch (result) {
        case 1:
            return "Bad";

        case 2:
            return "Worse";

        case 3:
            return "Average";

        case 4:
            return "Good";

        case 5:
            return "Great";
    }
}