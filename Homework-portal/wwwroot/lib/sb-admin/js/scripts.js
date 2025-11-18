// Minimal SB Admin interactions: sidebar toggle
(function(){
  const body = document.body;
  const sidebarToggle = document.getElementById('sidebarToggle');
  function toggle(){ body.classList.toggle('sb-sidenav-toggled'); }
  if(sidebarToggle){ sidebarToggle.addEventListener('click', toggle); }
})();