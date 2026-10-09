const API_BASE_URL = "http://localhost:7071/api";
const refreshButton = document.getElementById("refresh-button");
const reportBody = document.getElementById("report-body");
const statusMessage = document.getElementById("status-message");
const statusBadge = document.getElementById("status-badge");
const reportCard = document.getElementById("report-card");
const assignmentCount = document.getElementById("assignment-count");
const highestLevel = document.getElementById("highest-level");
const lastUpdated = document.getElementById("last-updated");
const syncDetail = document.getElementById("sync-detail");
const rowCount = document.getElementById("row-count");

function renderReport(rows) {
    const fragment = document.createDocumentFragment();
    for (const row of rows) {
        const tableRow = document.createElement("tr");
        for (const [key, value] of Object.entries({ no: row.no, playerName: row.playerName, level: row.level, age: row.age, assetName: row.assetName })) {
            const cell = document.createElement("td");
            if (key === "playerName") {
                const identity = document.createElement("div");
                identity.className = "player-identity";
                const avatar = document.createElement("span");
                avatar.className = "player-avatar";
                avatar.setAttribute("aria-hidden", "true");
                avatar.textContent = String(value).trim().split(/\s+/).map(part => part[0]).slice(0, 2).join("").toUpperCase();
                const info = document.createElement("span");
                info.className = "player-info";
                const name = document.createElement("span");
                name.textContent = value;
                const designation = document.createElement("small");
                designation.textContent = "PLAYER / " + String(row.no).padStart(2, "0");
                info.append(name, designation);
                identity.append(avatar, info);
                cell.append(identity);
            } else if (key === "level" || key === "assetName") {
                const label = document.createElement("span");
                label.className = key === "level" ? "level-pill" : "asset-name";
                label.textContent = value;
                cell.append(label);
            } else {
                cell.textContent = value;
            }
            tableRow.append(cell);
        }
        fragment.append(tableRow);
    }
    reportBody.replaceChildren(fragment);
}

async function loadReport() {
    refreshButton.disabled = true;
    reportCard.setAttribute("aria-busy", "true");
    statusMessage.hidden = false;
    statusMessage.className = "status-message";
    statusMessage.textContent = "Loading player asset data…";
    statusBadge.textContent = "Refreshing";
    statusBadge.dataset.state = "loading";
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 15000);
    try {
        const response = await fetch(`${API_BASE_URL}/getassetsbyplayer`, { signal: controller.signal, cache: "no-store" });
        if (!response.ok) throw new Error("Request failed");
        const result = await response.json();
        if (!result.success || !Array.isArray(result.data)) throw new Error("Invalid response");
        renderReport(result.data);
        assignmentCount.textContent = result.data.length;
        highestLevel.textContent = result.data.length ? result.data.reduce((highest, row) => Math.max(highest, row.level), 0) : "—";
        lastUpdated.textContent = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" });
        syncDetail.textContent = "Latest data loaded successfully";
        rowCount.textContent = `${result.data.length} assignment${result.data.length === 1 ? "" : "s"}`;
        statusBadge.textContent = "Up to date";
        statusBadge.dataset.state = "success";
        statusMessage.textContent = result.data.length ? `${result.data.length} assignments loaded.` : "No player asset data found.";
        statusMessage.hidden = result.data.length > 0;
    } catch {
        reportBody.replaceChildren();
        assignmentCount.textContent = "—";
        highestLevel.textContent = "—";
        rowCount.textContent = "Data unavailable";
        syncDetail.textContent = "Refresh to try again";
        statusBadge.textContent = "Connection failed";
        statusBadge.dataset.state = "error";
        statusMessage.className = "status-message error";
        statusMessage.textContent = "Unable to load player asset data. Please check the API connection and try again.";
    } finally {
        clearTimeout(timeout);
        refreshButton.disabled = false;
        reportCard.setAttribute("aria-busy", "false");
    }
}

refreshButton.addEventListener("click", loadReport);
loadReport();
