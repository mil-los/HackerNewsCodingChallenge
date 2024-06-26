import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'searchFilter' })
export class FilterPipe implements PipeTransform {

  transform(items: any[], searchText: string): any[] {
    if (!items) {
      return [];
    }
    if (!searchText) {
      return items;
    }
    searchText = searchText.toLocaleUpperCase();

    return items.filter(it => {
      return it?.title?.toLocaleUpperCase().includes(searchText);
    });
  }
}
