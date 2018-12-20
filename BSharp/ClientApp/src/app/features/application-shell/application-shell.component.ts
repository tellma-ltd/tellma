import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html',
  styleUrls: ['./application-shell.component.css']
})
export class ApplicationShellComponent implements OnInit {

  // For the menu
  public isCollapsed = true;

  constructor() { }

  ngOnInit() {
  }

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }
}
