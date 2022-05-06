import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.scss']
})
export class TestErrorsComponent implements OnInit {
  endpoint = 'https://localhost:5001/api/';
  validationErrors: string[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
  }

  post400ValidationError() {
    this.http.post(this.endpoint + 'account/register', {}).subscribe({
      next: response => console.log(response),
      error: error => {console.log(error); this.validationErrors = error},
    })
  }

  get400Error() {
    this.http.get(this.endpoint + 'error/bad-request').subscribe({
      next: response => console.log(response),
      error: error => console.log(error),
    })
  }

  get401Error() {
    this.http.get(this.endpoint + 'error/auth').subscribe({
      next: response => console.log(response),
      error: error => console.log(error),
    })
  }

  get404Error() {
    this.http.get(this.endpoint + 'error/not-found').subscribe({
      next: response => console.log(response),
      error: error => console.log(error),
    })
  }

  get500Error() {
    this.http.get(this.endpoint + 'error/server-error').subscribe({
      next: response => console.log(response),
      error: error => console.log(error),
    })
  }

}
