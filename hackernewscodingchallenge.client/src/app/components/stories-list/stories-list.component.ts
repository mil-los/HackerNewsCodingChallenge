import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Story } from '../../models/story';

@Component({
  selector: 'app-fetch-stories',
  templateUrl: './stories-list.component.html',
  styleUrl: './stories-list.component.css'
})
export class StoriesListComponent {
  public stories: Story[] = [];
  public searchText: string = '';
  public page: any;
  public http: HttpClient;
  public baseUrl: string;
  public isLoading: boolean = false;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.http.get<Story[]>(this.baseUrl + 'hackernews').subscribe(result => {
      this.stories = result;
      this.isLoading = false;
    }, error => {
      console.error(error);
      this.isLoading = false;
    });
  }

  onChange() {
    this.page = 1;
  }
}
