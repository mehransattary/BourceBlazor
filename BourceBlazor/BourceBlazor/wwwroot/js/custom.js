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
}
function SetOpacityFull() {
    document.querySelector('.gridNomadActions').classList.add("opacity1");
}