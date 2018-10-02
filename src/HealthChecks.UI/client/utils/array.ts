const chunkArray = <T>(array: Array<T>, chunk_size: number): T[][] => {
    var index = 0;
    var arrayLength = array.length;
    var tempArray = [];

    for (index = 0; index < arrayLength; index += chunk_size) {
        let chunk = array.slice(index, index + chunk_size);
        tempArray.push(chunk);
    }

    return tempArray;
}

export { chunkArray };