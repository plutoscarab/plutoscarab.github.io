---
title: Why Getting Rid of SRID from GeoJson was a Bad Idea
tags: [ GIS ]
---

There is a lot of misunderstanding about geometry vs geography types in GIS libraries like in
PostGIS and SQL Server. This misunderstanding leads to bad practices and corruption of GIS data that
may go unnoticed. The current GeoJson specification contributes to this problem.

Let's say we have an OGC-standard-compliant line segment `LINESTRING(0 10, 50 40)` and we want to
know what points are included on this line segment. (There are an infinite number of them.) The OGC
specification and GeoJson specification both agree that we must use linear interpolation. Pick a
value $$0 \le t \le 1$$ and this line segment becomes all of the points with $$x = 50t$$ and $$y =
10 + 30t$$. If $$t = 0$$ we get the starting point `0 10` and if $$t = 1$$ we get the ending point
`50 40`. The midpoint is given by $$t = \frac 1 2$$ which gives us `25 25`.

So let's take a look at the following GeoJson on a popular web mapping API:

```json
{
  "type": "GeometryCollection",
  "geometries": [
    {
      "type": "LineString",
      "coordinates": [[0, 10], [50, 40]]
    },
    {
      "type": "Point",
      "coordinates": [25, 25]
    }
  ]
}
```

Copy and paste this into [GeoJson.io](https://geojson.io). Zoom in. That point is supposed to be on
that line. The OGC and GeoJson specifications leave no doubt about this. But it's way off. Lots of
kilometers.

## Why It's Wrong

The web page is drawing the line segment as a straight line on a Web Mercator map. GeoJson has
nothing to do with Web Mercator so there's no reason to think that the coordinates contained on the
line would follow a straight path in Web Mercator. That's where SRID and spatial reference system
comes in. That's why we need that in GeoJson. 

I'm not picking on GeoJson.io in particular. Most web SDKs do the same wrong thing. Even some
sophisticated GIS tools have this error. QGIS has this error. 

If you want a straight line in Mercator, you should use a Mercator projection SRID and use
coordinates for that projection, which will usually be in meters. And that means you can't correctly
use official GeoJson. There was an older version of GeoJson that let you specify the SRS. Use that.

"But my users won't understand!" you might say. Well, you're not helping them understand, that's for
sure. You're helping them misunderstand more things.

"GeoJson is just a serialization format for my vertices!" you might say. Not according to the
specification it's not. It's right and important that the spec spells out which points are contained
on a given line segment. Without that information, you don't know what shape you're dealing with.
You won't know whether a point is inside a polygon, or which side of a line segment it's on.
Unfortunately, removing SRID in the spec tooks away the flexibility that the original GeoJson had.

The reason given for removing GeoJson was the clients couldn't be expected to have the data and
logic needed to interpret it correctly, so they essentially hard-coded SRID=4326. I don't think they
took into account the interoperability problem they created in place of that one, and how pervasive
the problem would be. Now clients aren't exposed to the SRID concept at all. There's no signal to
indicate that they might not be getting the full picture. The "linear interpolation" section of the
spec could certainly be amplified with examples to show how GeoJson is used correctly and
incorrectly.

## Geometry vs Geography, and Densification

If you convert data back and forth between geometry and geography types, you'll notice that the
coordinate values of your vertices don't change. So what's the difference? The difference is in the
points in between each vertex. For geometry, the points use linear interpolation. For geography,
they use geodesic interpolation. Only geometry does the right thing as far as OGC SQL/MM and GeoJson
standards are concerned.

"But I need to calculate distances!" Fine, use geography for points for when you need distance
calculations. But don't expect correct distance answers for geography objects that are linestrings
or polygons. If you need correct distances between linestrings, polygons, and other objects, you
first need to check if you need densification.

"But I want it to be straight in this random projection!" Then you need densification.

Densification involves adding additional vertices in between any two vertices that are far enough
apart where it makes a difference. Fortunately for most data, it doesn't make a difference. That's
why people tolerate this problem everywhere. Most line segments are short enough that it doesn't
matter. It's only these really long line segments like in the example above where it matters. But
you should verify that it doesn't matter for the data you're dealing with, and in the libraries that
you write.

Some software vendors are starting to realize that it matters. [Azure Cosmos
DB](https://azure.microsoft.com/en-us/services/cosmos-db/) used to only use the geography data type
under the covers. Some customers probably complained, because they've added support for the
[geometry data type](https://azure.microsoft.com/en-us/services/cosmos-db/). But part of the problem
remains: the geography type isn't compatible with the GeoJson specification.

"Why are the standards so dumb!? Geography is obviously better!" You'll have to take that one up
with the standards bodies and decades of GIS practices. Consider this: the geometry type is
mathematically simpler. It's much easier to implement spatial operations and indexing correctly, and
the implementations are generally much faster for geometry than for geography. And considering that
for _most_ data it doesn't matter anyway, we might as well use the easy and fast math. We just have
to be careful to understand where it does matter so that we can deal with it. And that's what the
standards do; they instruct us to use geometry, not geography, giving us the benefit of simple and
fast calculations, and tipping us off that we need to densify if we want geography-like behavior.

"But Earth isn't flat! Geometry is wrong!" It's true that the Earth isn't flat, but for much
real-life GIS data it might as well be. There's no reason to complicate and slow down our
computations just for the edge case of applications that need geodesic paths. Those edge cases are
the ones that should be adapting to the normal case, via the use of densification.
