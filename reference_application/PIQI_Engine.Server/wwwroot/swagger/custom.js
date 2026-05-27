//came from https://cavedweller92.wordpress.com/tag/swashbuckle-aspnetcore/
(function () {
    window.addEventListener("load", function () {
        setTimeout(() => {
            // Set Logo
            addLogoHeader();
        }, 1);
    });

    function addLogoHeader()
    {
        var logoLink = document.getElementsByClassName('link')[0];
        if (!logoLink) return;

        // Remove the existing Swagger logo (SVG or any child)
        while (logoLink.firstChild) {
            logoLink.removeChild(logoLink.firstChild);
        }

        // Create a new <img> element
        var imgElement = document.createElement('img');
        imgElement.alt = "PIQI Framework";
        imgElement.src = "https://piqiframework.org/wp-content/uploads/2024/11/PIQI-Framework-Logo-2024.png";
        imgElement.style.height = "80px";
        logoLink.appendChild(imgElement);

        // Link to clinicalarchitecture.com
        logoLink.href = "https://piqiframework.org/";
        logoLink.target = "_blank";
    }
})();