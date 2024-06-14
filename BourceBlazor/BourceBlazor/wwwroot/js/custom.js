Chart.defaults.font.family = 'Vazir';

IsSHowMenu = true;
function ShowMenu() {

    if (this.IsSHowMenu == true) {
        document.querySelector('.sidebar').style.display = 'none';

        this.IsSHowMenu = false;
    }
    else {
        document.querySelector('.sidebar').style.display = 'block';
        this.IsSHowMenu = true;
    }  
}


function SetOpacity_3() {
    document.querySelector('.gridNomadActions').classList.add("opacity_3");
    document.querySelector('.gridNomadActions').classList.remove("opacity1");
}
function SetOpacityFull() {
    document.querySelector('.gridNomadActions').classList.add("opacity1");
    document.querySelector('.gridNomadActions').classList.remove("opacity_3");
}