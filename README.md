GPUSpline
-------

A GPU accelerated 2D catmull-rom spline renderer optimized for mobile devices.

Renders hundreds of high-poly dynamic splines at 60fps on gles2 devices with low CPU usage. Spline generation is defered to the vertex phase without compute or geometry shaders.

Usage
-------

// add a single spline<br />
*SplineBatcher.Add(Vector2[], int numVertices))*<br />
// generate the batches<br />
*SplineBatcher.Generate()*<br />
// animate the splines at runtime<br />
*SplineBatcher.Modify(Vector2[])*<br />

System Requirements
-------

OpenGLES2.0 compatible devices or better, Unity 5.4+
