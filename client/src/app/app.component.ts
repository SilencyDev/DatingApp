import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'client';
  users: any[];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.getUsers();
  }

  getUsers(): void {
    this.http.get('https://localhost:5001/api/users').subscribe({
      next: (response: any[]) => this.users = response,
      error: (error: any) => console.log(error)
    });
  }
}
