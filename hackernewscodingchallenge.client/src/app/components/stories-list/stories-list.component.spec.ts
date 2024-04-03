import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StoriesListComponent } from './stories-list.component';
import { Story } from '../../models/story';
import { NgxPaginationModule } from 'ngx-pagination';

describe('StoriesListComponent', () => {
  let component: StoriesListComponent;
  let fixture: ComponentFixture<StoriesListComponent>;
  let httpTestingController: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, NgxPaginationModule],
      declarations: [StoriesListComponent],
      providers: [{ provide: 'BASE_URL', useValue: 'https://localhost:4200/' }]
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StoriesListComponent);
    component = fixture.componentInstance;
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should fetch stories on init', () => {
    const stories: Story[] = [
      { id: 1, title: 'Story 1', url: 'https://someUrl1.com/story1' },
      { id: 2, title: 'Story 2', url: 'https://someUrl2.com/story2' }
    ];

    component.ngOnInit();

    const req = httpTestingController.expectOne('http://localhost:5223/hackernews');
    expect(req.request.method).toEqual('GET');

    req.flush(stories);

    expect(component.stories).toEqual(stories);
  });
});
