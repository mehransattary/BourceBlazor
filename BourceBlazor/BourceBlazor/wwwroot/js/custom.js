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