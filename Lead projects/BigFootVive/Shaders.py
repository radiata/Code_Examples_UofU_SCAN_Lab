import viz

#~~~~~~~~~~Shader Set up~~~~~~~~#
# Red Shader
redFragCode = """
void main()
{
    gl_FragColor = vec4(171./256.,43./256.,43./256.,1.0);
}"""
redShader = viz.addShader(frag=redFragCode)

# Blue Shader
blueFragCode = """
void main()
{
    gl_FragColor = vec4(43./256.,43./256.,171./256.,1.0);
}"""
blueShader = viz.addShader(frag=blueFragCode)

