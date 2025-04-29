import { put } from "@vercel/blob"
import { NextResponse } from "next/server"

export const config = {
  runtime: "edge",
}

export default async function handler(request) {
  // Check if the request method is POST
  if (request.method !== "POST") {
    return new NextResponse(JSON.stringify({ error: "Method not allowed" }), {
      status: 405,
      headers: { "Content-Type": "application/json" },
    })
  }

  // Check authorization
  const authHeader = request.headers.get("authorization")
  if (!authHeader || !authHeader.startsWith("Bearer ")) {
    return new NextResponse(JSON.stringify({ error: "Unauthorized" }), {
      status: 401,
      headers: { "Content-Type": "application/json" },
    })
  }

  const token = authHeader.split(" ")[1]
  if (token !== process.env.BLOB_READ_WRITE_TOKEN) {
    return new NextResponse(JSON.stringify({ error: "Invalid token" }), {
      status: 401,
      headers: { "Content-Type": "application/json" },
    })
  }

  try {
    const formData = await request.formData()
    const file = formData.get("file")

    if (!file) {
      return new NextResponse(JSON.stringify({ error: "No file provided" }), {
        status: 400,
        headers: { "Content-Type": "application/json" },
      })
    }

    // Upload to Vercel Blob
    const blob = await put(file.name, file, {
      access: "public",
    })

    return new NextResponse(JSON.stringify({ url: blob.url }), {
      status: 200,
      headers: { "Content-Type": "application/json" },
    })
  } catch (error) {
    console.error("Error uploading to Vercel Blob:", error)
    return new NextResponse(JSON.stringify({ error: error.message }), {
      status: 500,
      headers: { "Content-Type": "application/json" },
    })
  }
}
