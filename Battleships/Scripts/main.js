
function showBoards() {
    $("#playerboard").html(showBoard("playerTile", "tile"));
    $("#AIboard").html(showBoard("AITile", "clickableTile"));
    $("#AIboard").html(attachEventToAITiles());
}

function showBoard(tileId, tileClass) {
    let divContent = "";
    for (let i = 0; i < 100; i++) {
        divContent += '<div id="' + tileId + i + '" class="' + tileClass + '"></div>';
        if ((i + 1) % 10 == 0 && i != 0) {
            divContent += '<div style="clear:both"></div>';
        }
    }
    return divContent;
}

function attachEventToAITiles() {
    for (let i = 0; i < 100; i++) {
        $("#AITile" + i).on("click", function () { shoot(i) });
    }
}

function getRandomShips() {
    if ($('#randomShips').hasClass("horizontalMenuButton")) {
        fetch(getQueryPath("Home/GetRandomShips"))
        .then(function (resp) {
            return resp.json();
        })
        .then(function (ships) {
            placePlayerShips(ships);
        });

        if ($("#startGame").hasClass("unclickableButton")) {
            changeButtonAccess("startGame");
        }
    }
}

function placePlayerShips(idArray) {
    for (i = 0; i < 100; i++) {
        revealTile("playerTile" + i, "water");
    }
    for (i = 0; i < idArray.length; i++) {
        revealTile("playerTile" + idArray[i], "ship");
        shipArray[i] = idArray[i];
    }
}

function revealTile(id, type) {
    var chosenType;
    if (type == "missed") {
        $("#historyInfo").html(getActionTime()
            + "<span style='color: green'> -- Missed. Computer's turn. Shot count: "
            + gameSequence + "<br/></span>"
            + $("#historyInfo").html());
        chosenType = "url(../img/missed.png)";
    }
    else if (type == "shot") {
        $("#historyInfo").html(getActionTime()
            + "<span style='color: green'> -- Hit! You have one more try. Shot count: "
            + gameSequence + "<br/></span>"
            + $("#historyInfo").html());
        chosenType = "url(../img/shot.png)";
    }
    else if (type == "ship") {
        chosenType = "url(../img/ship.png)";
    }
    else if (type == "water") {
        chosenType = "url(../img/water.png)";
    }
    else if (type == "drowned") {
        chosenType = "url(../img/drowned.png)";
    }
    else {
        return;
    }

    if ($("#" + id).hasClass("clickableTile")) {
        $("#" + id).removeClass("clickableTile");
        $("#" + id).addClass("tile");
    }
    $("#" + id).css('backgroundImage', chosenType);
}

function changeSectionDisplay(vanishDiv, displayDiv) {
    $("#" + vanishDiv).css('display', 'none');
    $("#" + displayDiv).css('display', 'block');
}

function startGame() {
    if ($('#startGame').hasClass("horizontalMenuButton")) {
        fetch(getQueryPath("Home/StartGame"),
    {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify(shipArray)
    })
    .then(function (resp) {
        return resp.json();
    })
    .then(function (number) {
        gameNumber = number;
    });

        changeButtonAccess("startGame");
        changeButtonAccess("addShip");
        changeButtonAccess("randomShips");
        changeButtonAccess("newGame");
        changeButtonAccess("randomShot");

        let startDate = new Date();
        startTime = startDate.getTime() / 1000;

        $("#historyInfo").html(getActionTime() + " -- START<br/>" + $("#historyInfo").html());
    }
}

function startNewGame() {
    if ($('#newGame').hasClass("horizontalMenuButton")) {

        showBoards();

        changeButtonAccess("addShip");
        changeButtonAccess("randomShips");
        changeButtonAccess("newGame");
        if ($("#randomShot").hasClass("horizontalMenuButton")) {
            changeButtonAccess("randomShot");
        }

        playerWon = false;
        AIWon = false;

        $("#historyInfo").html("New game<br/>" + $("#historyInfo").html());
    }
}

function changeButtonAccess(buttonId) {
    if ($('#' + buttonId).hasClass("horizontalMenuButton")) {
        $('#' + buttonId).removeClass("horizontalMenuButton");
        $('#' + buttonId).addClass("unclickableButton");
    }
    else {
        $('#' + buttonId).removeClass("unclickableButton");
        $('#' + buttonId).addClass("horizontalMenuButton");
    }
}

function shoot(index) {
    if ($('#randomShot').hasClass("horizontalMenuButton") && tileLock == false) {
        fetch(getQueryPath("Home/Shoot"),
        {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({ gameNumber: gameNumber, index: index })
        })
        .then(function (resp) {
            return resp.json();
        })
        .then(function (list) {
            resolveShot(list);
        });
    }
}

function shootRandomly() {
    if ($('#randomShot').hasClass("horizontalMenuButton") && tileLock == false) {
        fetch(getQueryPath("Home/ShootRandomly"),
        {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({ gameNumber: gameNumber })
        })
        .then(function (resp) {
            return resp.json();
        })
        .then(function (list) {
            resolveShot(list);
        });
    }
}

function resolveShot(list) {
    gameSequence = list[0].Sequence;
    tileLock = true;
    for (i = 0; i < list.length; i++) {
        setTimeout(function (list, i) {            
            revealTile("AITile" + list[i].Index, list[i].Tile);
            if (i == list.length - 1) {
                tileLock = false;
            }
        }, 300 * i, list, i);
    }
    if (list[0].Tile == "drowned") {
        gameSequence = list[0].Sequence;
        checkVictoryOrDefeat("aiTile");
    }
    if (list[0].Tile == "missed" && playerWon == false) {
        aiShot();
    }
}

function aiShot() {
    fetch(getQueryPath("Home/AIShot"),
        {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({ gameNumber: gameNumber })
        })
        .then(function (resp) {
            return resp.json();
        })
        .then(function (list) {

            tileLock = true;
            for (i = 0; i < list.length; i++) {
                setTimeout(function (list, i) {
                    markPlayerTiles(list[i].Tile, list[i].Index, list[i].IsVisible);
                    if (i == list.length - 1) {
                        tileLock = false;
                        if (list.map(function (item) {
                         return item.Tile;
                        }).includes("drowned")) {
                            checkVictoryOrDefeat("playerTile");
                        }
                    }
                }, 300 * (i + 1), list, i);
            }
        });
}

function markPlayerTiles(tile, index, visible) {
    var chosenType;
    if (tile == "missed") {
        chosenType = "url(../img/missed.png)";
        $("#historyInfo").html(getActionTime()
            + "<span style='color: red'> -- Missed. Your turn!<br/></span>"
            + $("#historyInfo").html());
    }
    else if (tile == "shot") {
        chosenType = "url(../img/shot.png)";
        $("#historyInfo").html(getActionTime()
            + "<span style='color: red'> -- Hit! Computer gets another try.<br/></span>"
            + $("#historyInfo").html());

    }
    else if (tile == "drowned") {
        chosenType = "url(../img/drowned.png)";
        if (visible) {
            $("#historyInfo").html(getActionTime()
        + "<span style='color: red'> -- Drowned! Computer gets another try.<br/></span>"
        + $("#historyInfo").html());
        }
    }
    else {
        return;
    }

    $("#playerTile" + index).css('backgroundImage', chosenType);
}

function checkVictoryOrDefeat(shotAt) {
    fetch(getQueryPath("Home/CheckVictoryOrDefeat"),
        {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({ gameNumber: gameNumber, shotAt: shotAt })
        })
        .then(function (resp) {
            return resp.json();
        })
        .then(function (ended) {
            if (shotAt == "aiTile" && ended) {
                $("#historyInfo").html(getActionTime()
                    + " -- PLAYER WINS AT "
                    + gameSequence
                    + " SHOTS!<br/>"
                    + $("#historyInfo").html());
                playerWon = true;
                removeEventFromAITiles();
            }
            else if (shotAt == "playerTile" && ended) {
                $("#historyInfo").html(getActionTime()
                    + " -- COMPUTER WINS !<br/>"
                    + $("#historyInfo").html());
                AIWon = true;
                removeEventFromAITiles();
            }
            else if (shotAt == "aiTile" && !ended) {
                $("#historyInfo").html(getActionTime()
                    + "<span style='color: green'> -- Drowned! You have one more try. Shot count: "
                    + gameSequence + "<br/></span>"
                    + $("#historyInfo").html());
            }
        });
}

function removeEventFromAITiles() {

    changeButtonAccess("randomShot");

    for (let i = 0; i < 100; i++) {
        if ($("#AITile" + i).hasClass("clickableTile")) {

            $("#AITile" + i).removeClass("clickableTile");
            $("#AITile" + i).addClass("tile");

            $("#AITile" + i).css('backgroundImage', 'url(../img/covered.png)');
        }
    }
}

function getActionTime() {
    let actionDate = new Date();
    actionTime = actionDate.getTime() / 1000;

    let time = actionTime - startTime;
    let hour = Math.floor(time / 3600);
    if (hour < 10)
        hour = "0" + hour;
    let minute = Math.floor(time % 3600 / 60);
    if (minute < 10)
        minute = "0" + minute;
    let second = Math.floor(time % 3600 % 60);
    if (second < 10)
        second = "0" + second;

    if (hour == 0)
        return minute + " : " + second;
    else
        return hour + " : " + minute + " : " + second;
}

function getQueryPath(address) {
    return window.location.protocol + "//" + window.location.host + "/" + address;
}

$("#playButton").on("click", function () { changeSectionDisplay("mainMenuSection", "boardSection") });
//$("#rankingButton").on("click", function () { changeSectionDisplay("mainMenuSection", "ranking") });
$("#aboutButton").on("click", function () { changeSectionDisplay("mainMenuSection", "aboutSection") });

$("#randomShips").on("click", function () { getRandomShips() });
$("#startGame").on("click", function () { startGame() });
$("#randomShot").on("click", function () { shootRandomly() });
$("#newGame").on("click", function () { startNewGame() });
$("#goToMainMenu").on("click", function () { changeSectionDisplay("boardSection", "mainMenuSection"); startNewGame(); $("#historyInfo").html(""); });
$("#goFromAboutToMainMenu").on("click", function () { changeSectionDisplay("aboutSection", "mainMenuSection") });

showBoards();

var shipArray = [];
var gameNumber;
var gameSequence;
var playerWon = false;
var AIWon = false;
var tileLock = false;

var startHour;
var startMinute;
var startSecond;

