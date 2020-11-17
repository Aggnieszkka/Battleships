
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
    for (let i = 0; i < 100; i++) {
        revealTile("playerTile" + i, "water", 0);
    }
    for (let i = 0; i < idArray.length; i++) {
        revealTile("playerTile" + idArray[i], "ship", 0);
        shipArray[i] = idArray[i];
    }
}

function revealTile(id, type, time) {
    let chosenType;
    if (type == "missed") {
        $("#historyInfo").html(time
            + "<span style='color: green'> -- Missed. Computer's turn. Shot count: "
            + gameSequence + "<br/></span>"
            + $("#historyInfo").html());
        chosenType = "url(../img/missed.png)";
    }
    else if (type == "shot") {
        $("#historyInfo").html(time
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

        $("#historyInfo").html("00 : 00 -- START<br/>" + $("#historyInfo").html());
    }
}

function startNewGame() { 
    if ($('#newGame').hasClass("horizontalMenuButton")) {

        showBoards();

        changeButtonAccess("addShip");
        changeButtonAccess("randomShips");
        changeButtonAccess("newGame");
        gameNumber = -1;
        if ($("#randomShot").hasClass("horizontalMenuButton")) {
            changeButtonAccess("randomShot");
        }

        playerWon = false;
        AIWon = false;
        tileLock = false;

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
    for (let i = 0; i < list.length; i++) {
        setTimeout(function (list, i) {
            revealTile("AITile" + list[i].Index, list[i].Tile, list[i].Time);
            if (i == list.length - 1) {
                tileLock = false;
            }
        }, 300 * i, list, i);
    }
    if (list[0].Tile == "drowned") {
        gameSequence = list[0].Sequence;
        checkVictoryOrDefeat("aiTile", list[0].Time);
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
            for (let i = 0; i < list.length; i++) {
                setTimeout(function (list, i) {
                    if (list[i].GameNumber == gameNumber) {
                        markPlayerTiles(list[i].Tile, list[i].Index, list[i].IsVisible, list[i].Time);
                        if (i == list.length - 1) {
                            tileLock = false;
                            if (list.map(function (item) {
                             return item.Tile;
                            }).includes("drowned")) {
                                checkVictoryOrDefeat("playerTile", list[0].Time);
                            }
                        }
                    }
                }, 300 * (i + 1), list, i);
            }
        });
}

function markPlayerTiles(tile, index, visible, time) {
    let chosenType;
    if (tile == "missed") {
        chosenType = "url(../img/missed.png)";
        $("#historyInfo").html(time
            + "<span style='color: red'> -- Missed. Your turn!<br/></span>"
            + $("#historyInfo").html());
    }
    else if (tile == "shot") {
        chosenType = "url(../img/shot.png)";
        $("#historyInfo").html(time
            + "<span style='color: red'> -- Hit! Computer gets another try.<br/></span>"
            + $("#historyInfo").html());

    }
    else if (tile == "drowned") {
        chosenType = "url(../img/drowned.png)";
        if (visible) {
            $("#historyInfo").html(time
        + "<span style='color: red'> -- Drowned! Computer gets another try.<br/></span>"
        + $("#historyInfo").html());
        }
    }
    else {
        return;
    }

    $("#playerTile" + index).css('backgroundImage', chosenType);
}

function checkVictoryOrDefeat(shotAt, time) {
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
                $("#historyInfo").html(time
                    + " -- PLAYER WINS AT "
                    + gameSequence
                    + " SHOTS!<br/>"
                    + $("#historyInfo").html());
                playerWon = true;
                removeEventFromAITiles();
                qualifyForRanking();
            }
            else if (shotAt == "playerTile" && ended) {
                $("#historyInfo").html(time
                    + " -- COMPUTER WINS !<br/>"
                    + $("#historyInfo").html());
                AIWon = true;
                removeEventFromAITiles();
            }
            else if (shotAt == "aiTile" && !ended) {
                $("#historyInfo").html(time
                    + "<span style='color: green'> -- Drowned! You have one more try. Shot count: "
                    + gameSequence + "<br/></span>"
                    + $("#historyInfo").html());
            }
        });
}

function qualifyForRanking() {
    return fetch(getQueryPath("Home/QualifyForRanking"),
        {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({ gameNumber: gameNumber })
        })
        .then(function (resp) {
            return resp.json();
        })
        .then(function (qualified) {
            if (qualified) {
                $('#nicknameModal').modal({ backdrop: false });
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

function getQueryPath(address) {
    return window.location.protocol + "//" + window.location.host + "/" + address;
}

function updateRanking() {
    getRankingRows().then(function (rows) { $("#rankingTableContent").html(rows); });
}

function getRankingRows() {

    return fetch(getQueryPath("Home/GetRankingRows"))
    .then(function (resp) {
        return resp.json();
    })
    .then(function (rankingRows) {

        let divContent = "";
        let cellClass = "";
        let number = "";
        let nick = "";
        let shots = "";
        let time = "";
        let date = "";

        for (let i = 0; i < 8; i++) {
            if (rankingRows != "" && rankingRows.length > i) {
                number = i + 1;
                nick = rankingRows[i].Nick;
                shots = rankingRows[i].Shots;
                time = rankingRows[i].Time;
                date = rankingRows[i].Date;
            }
            else {
                number = "";
                nick = "";
                shots = "";
                time = "";
                date = "";
            }
            if (i % 2 == 0) {
                cellClass = "darkCell";
            }
            else {
                cellClass = "brightCell";
            }
            divContent += "<div id='row" + i + "' class='row'>"
                                                + "<div class='" + cellClass + " col-sm-1' style='border:none'></div>"
                                                + "<div class='" + cellClass + " numberCell col-sm-1'>" + number + "</div>"
                                                + "<div class='" + cellClass + " col-mid-2 col-sm-3'>" + nick + "</div>"
                                                + "<div class='" + cellClass + " col-sm-2'>" + shots + "</div>"
                                                + "<div class='" + cellClass + " col-sm-2'>" + time + "</div>"
                                                + "<div class='" + cellClass + " col-sm-2' style='border-right:none'>" + date + "</div>"
                                                + "<div class='" + cellClass + " col-mid-2 col-sm-1' style='border:none'></div>"
                                + "</div>";
        }
        return divContent;
    });
}
function setNickname(nickname) {
    fetch(getQueryPath("Home/SetNickname"),
{
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ gameNumber: gameNumber, nickname: nickname })
})
}

$("#playButton").on("click", function () { changeSectionDisplay("mainMenuSection", "boardSection") });
$("#rankingButton").on("click", function () { changeSectionDisplay("mainMenuSection", "rankingSection"); updateRanking(); });
$("#aboutButton").on("click", function () { changeSectionDisplay("mainMenuSection", "aboutSection") });

$("#randomShips").on("click", function () { getRandomShips() });
$("#startGame").on("click", function () { startGame() });
$("#randomShot").on("click", function () { shootRandomly() });
$("#newGame").on("click", function () { startNewGame() });
$("#goToMainMenu").on("click", function () { changeSectionDisplay("boardSection", "mainMenuSection"); startNewGame(); $("#historyInfo").html(""); });
$("#goFromRankingToMainMenu").on("click", function () { changeSectionDisplay("rankingSection", "mainMenuSection") });
$("#goFromAboutToMainMenu").on("click", function () { changeSectionDisplay("aboutSection", "mainMenuSection") });
$("#okButton").on("click", function () { setNickname($("#nickname").val()); $("#nicknameModal").modal('toggle'); });

showBoards();
updateRanking();

let shipArray = [];
let gameNumber;
let gameSequence;
let playerWon = false;
let AIWon = false;
let tileLock = false;

let startHour;
let startMinute;
let startSecond;