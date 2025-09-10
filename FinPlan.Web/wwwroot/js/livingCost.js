window.livingCost = (function () {
    function setAllAccordion(expand) {
        const accordion = document.getElementById('bangaloreAccordion');
        if (!accordion) return;
        const items = accordion.querySelectorAll('.accordion-collapse');
        items.forEach(item => {
            if (expand) {
                if (!item.classList.contains('show')) {
                    const bs = bootstrap.Collapse.getOrCreateInstance(item);
                    bs.show();
                }
            }
            else {
                if (item.classList.contains('show')) {
                    const bs = bootstrap.Collapse.getOrCreateInstance(item);
                    bs.hide();
                }
            }
        });
    }

    return {
        setAllAccordion
    };
})();