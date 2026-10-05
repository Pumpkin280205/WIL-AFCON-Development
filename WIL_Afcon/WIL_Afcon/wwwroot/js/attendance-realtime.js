// The API address comes from configuration (ApiSettings:PublicUrl) via a
// <meta> tag in _Layout; fall back to the local development ports.
const apiPublicUrl = document
    .querySelector("meta[name='api-public-url']")
    ?.getAttribute("content");

const hubUrl = apiPublicUrl
    ? new URL("attendanceHub", apiPublicUrl).toString()
    : window.location.protocol === "https:"
        ? "https://localhost:7285/attendanceHub"
        : "http://localhost:5199/attendanceHub";

const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

connection.on("AttendanceUpdated", function (attendance) {
    console.log("Attendance updated:", attendance);
    window.location.reload();
});

connection.start()
    .then(function () {
        console.log("Connected to Attendance SignalR hub.");
    })
    .catch(function (err) {
        console.error("SignalR connection error:", err);
    });