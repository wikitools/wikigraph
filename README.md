# WikiGraph
Interactive 3D visualization of Wikipedia articles and categories in a graph-like structure focused on highlighting the interconnections.

## Getting started

### Running
- Clone project
- Open in Unity (2018.1.9f2 preferred)
- Basic settings are on ``GRAPH`` object in ``Graph Controller``
- By default only a small test node set is available - ``Load Test Node Set`` checkbox

### Loading full Wikipedia node set
- Download the [data files](https://drive.google.com/open?id=107n-Bm3Enm-WQO8gVqk42P70a9bcMWTR)
- Unpack to ``Assets/StreamingAssets/DataFiles`` folder
- Uncheck the ``Load Test Node Set`` option
- You might want to tweak the ``Node Loaded Limit`` - start with a couple hundred
- Adjust the ``World Radius`` accordingly

## Current look

![Screenshot_20191016_190428](https://user-images.githubusercontent.com/8643919/66942100-959e4580-f048-11e9-8f44-462c5f42f9c1.png)

![Screenshot_20191016_190851](https://user-images.githubusercontent.com/8643919/66942115-9afb9000-f048-11e9-8b99-7616167ed7d8.png)

## Development
- [Our Jira Project Board](https://wikigraph.atlassian.net)

### Unity version
- 2018.1.9f2
